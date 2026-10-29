using System;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginManager : MonoBehaviour
{
    [Header("Inputs")]
    public TMP_InputField inputID;
    public TMP_InputField inputPassword;
    public TextMeshProUGUI errorText;

    [Header("Settings")]
    [Tooltip("Give up and show an error if the server takes longer than this.")]
    public float timeoutSeconds = 15f;
    public string nextScene = "MainMenu";

    private GameObject loadingOverlay;
    private Image spinner;
    private TextMeshProUGUI loadingText;
    private string loadingMessage = "Logging in";
    private int lastDots = -1;

    private bool isLoggingIn;

    public void loginButtonClicked()
    {
        if (isLoggingIn) return; // ignore double taps

        string tempID = inputID.text.Trim();
        string tempPassword = inputPassword.text.Trim();

        login(tempID, tempPassword);
    }

    public async void login(string studentID, string studentPassword)
    {
        if (isLoggingIn) return;

        HideError();

        if (string.IsNullOrEmpty(studentID) || string.IsNullOrEmpty(studentPassword))
        {
            ShowError("Please enter your Student ID and password.");
            return;
        }

        isLoggingIn = true;
        inputID.DeactivateInputField();
        inputPassword.DeactivateInputField();
        ShowLoading("Logging in");

        bool success = false;
        try
        {
            // student number becomes an email, ex: 26003 -> 26003@app.local
            string email = studentID.ToLower() + "@app.local";

            try
            {
                await WithTimeout(SupabaseManager.Instance.supabase.Auth.SignIn(email, studentPassword));
            }
            catch (TimeoutException)
            {
                throw;
            }
            catch (Exception e)
            {
                Debug.LogWarning("Sign in failed: " + e.Message);
                ShowError("Wrong Student ID or password.");
                return;
            }

            // get the student's name and section (falls back if the table can't be read)
            string firstName = "Student";
            string fullName = "";
            string sectionName = "";
            string accountStatus = "active";
            try
            {
                var res = await WithTimeout(
                    SupabaseManager.Instance.supabase
                        .From<StudentRow>()
                        .Where(x => x.studentNumber == studentID)
                        .Get());

                StudentRow row = res.Models.FirstOrDefault();
                if (row != null)
                {
                    Debug.Log("student row: " + row.fullName + " | " + row.section);
                    if (!string.IsNullOrEmpty(row.fullName))
                    {
                        fullName = row.fullName.Trim();
                        firstName = fullName.Split(' ')[0];
                    }
                    sectionName = row.section ?? "";
                    accountStatus = row.status ?? "active";
                }
                else
                {
                    Debug.LogWarning("students table returned nothing for this student");
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("Could not read student row: " + e.Message);
            }

            // archived students can log in to Supabase Auth but every save is rejected, so stop them here
            if (accountStatus != "active")
            {
                try { await SupabaseManager.Instance.supabase.Auth.SignOut(); } catch (Exception) { }
                ShowError("This account is not active. Please contact your teacher.");
                return;
            }

            SessionManager.Instance.studentID = studentID;
            SessionManager.Instance.studentFirstName = firstName;
            SessionManager.Instance.studentLastName = fullName.Contains(" ") ? fullName.Substring(fullName.IndexOf(' ') + 1) : "";
            SessionManager.Instance.sectionName = sectionName;

            success = true;
            ShowLoading("Loading");
            SceneManager.LoadSceneAsync(nextScene);
        }
        catch (TimeoutException)
        {
            ShowError("Connection timed out. Please try again.");
        }
        catch (Exception e)
        {
            Debug.LogError("Login failed: " + e);
            ShowError("Something went wrong. Check your internet and try again.");
        }
        finally
        {
            if (!success)
            {
                HideLoading();
                isLoggingIn = false;
            }
        }
    }

    // ---------- helpers ----------

    private async Task<T> WithTimeout<T>(Task<T> task)
    {
        if (await Task.WhenAny(task, Task.Delay(TimeSpan.FromSeconds(timeoutSeconds))) != task)
            throw new TimeoutException();
        return await task; // rethrows the real exception if the request failed
    }

    private void ShowError(string message)
    {
        if (errorText == null) return;
        errorText.gameObject.SetActive(true);
        errorText.SetText(message);
    }

    private void HideError()
    {
        if (errorText != null) errorText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (loadingOverlay == null || !loadingOverlay.activeSelf) return;

        spinner.rectTransform.Rotate(0f, 0f, 360f * Time.unscaledDeltaTime);

        // "Logging in" -> "Logging in." -> ".." -> "..."
        int dots = (int)(Time.unscaledTime * 2.5f) % 4;
        if (dots != lastDots)
        {
            lastDots = dots;
            // invisible dots keep the text from jittering sideways
            loadingText.SetText(loadingMessage + new string('.', dots) + "<alpha=#00>" + new string('.', 3 - dots));
        }
    }

    // ---------- loading overlay ----------

    private void ShowLoading(string message)
    {
        BuildLoadingOverlay();
        loadingMessage = message;
        lastDots = -1;
        loadingOverlay.SetActive(true);
    }

    private void HideLoading()
    {
        if (loadingOverlay != null) loadingOverlay.SetActive(false);
    }

    private void BuildLoadingOverlay()
    {
        if (loadingOverlay != null) return;

        GameObject canvasGO = new GameObject("LoginLoadingOverlay");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>(); // lets the dim layer block taps underneath
        SceneManager.MoveGameObjectToScene(canvasGO, gameObject.scene);

        // dark full-screen layer (also blocks the buttons and fields)
        GameObject dimGO = new GameObject("Dim");
        dimGO.transform.SetParent(canvasGO.transform, false);
        Image dim = dimGO.AddComponent<Image>();
        dim.color = new Color(0f, 0f, 0f, 0.65f);
        RectTransform dimRT = dim.rectTransform;
        dimRT.anchorMin = Vector2.zero;
        dimRT.anchorMax = Vector2.one;
        dimRT.offsetMin = Vector2.zero;
        dimRT.offsetMax = Vector2.zero;

        // spinner
        GameObject spinGO = new GameObject("Spinner");
        spinGO.transform.SetParent(canvasGO.transform, false);
        spinner = spinGO.AddComponent<Image>();
        spinner.sprite = MakeRingSprite();
        spinner.color = new Color(1f, 0.85f, 0.45f, 1f); // gold, matches the UI
        spinner.raycastTarget = false;
        spinner.rectTransform.sizeDelta = new Vector2(170f, 170f);
        spinner.rectTransform.anchoredPosition = new Vector2(0f, 60f);

        // text
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(canvasGO.transform, false);
        loadingText = textGO.AddComponent<TextMeshProUGUI>();
        loadingText.alignment = TextAlignmentOptions.Center;
        loadingText.fontSize = 52f;
        loadingText.color = Color.white;
        loadingText.raycastTarget = false;
        if (errorText != null) loadingText.font = errorText.font; // reuse the game's font
        loadingText.rectTransform.sizeDelta = new Vector2(900f, 110f);
        loadingText.rectTransform.anchoredPosition = new Vector2(0f, -110f);

        loadingOverlay = canvasGO;
        loadingOverlay.SetActive(false);
    }

    // draws a ring with a fading tail so it looks like a spinner, no image asset needed
    private static Sprite MakeRingSprite()
    {
        const int size = 128;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;

        Vector2 c = new Vector2(size / 2f, size / 2f);
        float outer = size / 2f - 1f;
        float inner = outer - size * 0.12f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 p = new Vector2(x, y) - c;
                float d = p.magnitude;
                float a = 0f;

                if (d <= outer + 0.5f && d >= inner - 0.5f)
                {
                    float angle = Mathf.Atan2(p.y, p.x) * Mathf.Rad2Deg; // -180..180
                    a = (angle + 180f) / 360f;                           // fading tail
                    a *= Mathf.Clamp01(outer - d + 0.5f) * Mathf.Clamp01(d - inner + 0.5f);
                }
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }
}