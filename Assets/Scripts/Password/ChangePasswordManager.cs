using System;
using System.Linq;
using System.Threading.Tasks;
using Supabase.Gotrue;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChangePasswordManager : MonoBehaviour
{
    [Header("Inputs")]
    public TMP_InputField inputCurrentPassword;
    public TMP_InputField inputNewPassword;
    public TMP_InputField confirmNewPassword;
    public TextMeshProUGUI errorText;  

    [Header("Settings")]
    public string backScene = "Account";
    public int minLength = 8;
    public float timeoutSeconds = 15f;

    private GameObject overlay;
    private Image spinner;
    private TextMeshProUGUI overlayText;
    private string message = "";
    private bool animateDots;
    private int lastDots = -1;
    private bool busy;

    // hook to Button - Done
// hook to Button - Done
    public async void DoneClicked()
    {
        if (busy) return;
        HideError();

        string current = inputCurrentPassword.text.Trim();
        string newPw = inputNewPassword.text.Trim();
        string confirm = confirmNewPassword != null ? confirmNewPassword.text.Trim() : "";

        // 1. Check for empty fields (including confirmation)
        if (string.IsNullOrEmpty(current) || string.IsNullOrEmpty(newPw) || string.IsNullOrEmpty(confirm)) 
        { 
            ShowError("Please fill in all fields."); 
            return; 
        }

        // 2. Check if new password matches confirmation
        if (newPw != confirm) 
        { 
            ShowError("New passwords do not match."); 
            return; 
        }

        // 3. Existing validation rules
        if (newPw.Length < minLength) { ShowError("Password must be at least " + minLength + " characters."); return; }
        if (!newPw.Any(char.IsUpper)) { ShowError("Password needs at least 1 capital letter."); return; }
        if (!newPw.Any(char.IsDigit)) { ShowError("Password needs at least 1 number."); return; }
        if (newPw == current) { ShowError("New password must be different from the current one."); return; }

        busy = true;
        ShowOverlay("Changing password", true);
        bool success = false;

        try
        {
            var client = SupabaseManager.Instance.supabase;

            // email ng logged-in student (studentNumber@app.local)
            string email = client.Auth.CurrentUser != null ? client.Auth.CurrentUser.Email : null;
            if (string.IsNullOrEmpty(email) && SessionManager.Instance != null)
                email = SessionManager.Instance.studentID.ToLower() + "@app.local";

            // 4) check current password
            try { await WithTimeout(client.Auth.SignIn(email, current)); }
            catch (TimeoutException) { throw; }
            catch (Exception)
            {
                ShowError("Current password is wrong.");
                return;
            }

            // 5) set new password
            await WithTimeout(client.Auth.Update(new UserAttributes { Password = newPw }));

            success = true;
            ShowOverlay("Password changed!", false);
            await Task.Delay(1500);

            SceneManager.LoadScene(backScene);
        }
        catch (TimeoutException) { ShowError("Connection timed out. Please try again."); }
        catch (Exception e)
        {
            Debug.LogError("Change password failed: " + e);
            ShowError("Could not change password. Please try again.");
        }
        finally
        {
            if (!success)
            {
                if (overlay != null) overlay.SetActive(false);
                busy = false;
            }
        }
    }

    public void BackClicked()
    {
        if (busy) return;
        SceneManager.LoadScene(backScene);
    }

    private async Task<T> WithTimeout<T>(Task<T> task)
    {
        if (await Task.WhenAny(task, Task.Delay(TimeSpan.FromSeconds(timeoutSeconds))) != task)
            throw new TimeoutException();
        return await task;
    }

    private void ShowError(string msg)
    {
        if (errorText == null) { Debug.LogWarning(msg); return; }
        errorText.gameObject.SetActive(true);
        errorText.SetText(msg);
    }

    private void HideError() { if (errorText != null) errorText.gameObject.SetActive(false); }

    void Update()
    {
        if (overlay == null || !overlay.activeSelf) return;
        if (spinner.gameObject.activeSelf) spinner.rectTransform.Rotate(0f, 0f, 360f * Time.unscaledDeltaTime);
        if (!animateDots) return;

        int dots = (int)(Time.unscaledTime * 2.5f) % 4;
        if (dots != lastDots)
        {
            lastDots = dots;
            overlayText.SetText(message + new string('.', dots) + "<alpha=#00>" + new string('.', 3 - dots));
        }
    }

    private void ShowOverlay(string msg, bool loading)
    {
        Build();
        message = msg;
        animateDots = loading;
        lastDots = -1;
        spinner.gameObject.SetActive(loading);  
        if (!loading) overlayText.SetText(msg);
        overlay.SetActive(true);
    }

    private void Build()
    {
        if (overlay != null) return;

        GameObject canvasGO = new GameObject("ChangePasswordOverlay");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();
        SceneManager.MoveGameObjectToScene(canvasGO, gameObject.scene);

        GameObject dimGO = new GameObject("Dim");
        dimGO.transform.SetParent(canvasGO.transform, false);
        Image dim = dimGO.AddComponent<Image>();
        dim.color = new Color(0f, 0f, 0f, 0.65f);
        dim.rectTransform.anchorMin = Vector2.zero;
        dim.rectTransform.anchorMax = Vector2.one;
        dim.rectTransform.offsetMin = Vector2.zero;
        dim.rectTransform.offsetMax = Vector2.zero;

        GameObject spinGO = new GameObject("Spinner");
        spinGO.transform.SetParent(canvasGO.transform, false);
        spinner = spinGO.AddComponent<Image>();
        spinner.sprite = MakeRingSprite();
        spinner.color = new Color(1f, 0.85f, 0.45f, 1f);
        spinner.raycastTarget = false;
        spinner.rectTransform.sizeDelta = new Vector2(170f, 170f);
        spinner.rectTransform.anchoredPosition = new Vector2(0f, 60f);

        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(canvasGO.transform, false);
        overlayText = textGO.AddComponent<TextMeshProUGUI>();
        overlayText.alignment = TextAlignmentOptions.Center;
        overlayText.fontSize = 52f;
        overlayText.color = Color.white;
        overlayText.raycastTarget = false;
        if (errorText != null) overlayText.font = errorText.font;
        overlayText.rectTransform.sizeDelta = new Vector2(900f, 110f);
        overlayText.rectTransform.anchoredPosition = new Vector2(0f, -110f);

        overlay = canvasGO;
        overlay.SetActive(false);
    }

    private static Sprite MakeRingSprite()
    {
        const int size = 128;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        Vector2 c = new Vector2(size / 2f, size / 2f);
        float outer = size / 2f - 1f;
        float inner = outer - size * 0.12f;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                Vector2 p = new Vector2(x, y) - c;
                float d = p.magnitude, a = 0f;
                if (d <= outer + 0.5f && d >= inner - 0.5f)
                {
                    a = (Mathf.Atan2(p.y, p.x) * Mathf.Rad2Deg + 180f) / 360f;
                    a *= Mathf.Clamp01(outer - d + 0.5f) * Mathf.Clamp01(d - inner + 0.5f);
                }
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }
}