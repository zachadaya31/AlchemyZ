using UnityEngine;
using TMPro;

public class ElementListLoader : MonoBehaviour
{
    [Header("1. UI Settings")]
    [Tooltip("Drag your Button Prefab here")]
    public GameObject buttonPrefab;

    [Tooltip("Drag your Scroll Rect's 'Content' GameObject here")]
    public Transform contentParent;

    [Header("2. The List of Element Names")]
    public string[] elementNames = new string[]
    {
        "1\nHydrogen", "2\nHelium", "3\nLithium", "4\nBeryllium", "5\nBoron",
        "6\nCarbon", "7\nNitrogen", "8\nOxygen", "9\nFluorine", "10\nNeon",
        "11\nSodium", "12\nMagnesium", "13\nAluminium", "14\nSilicon", "15\nPhosphorus",
        "16\nSulfur", "17\nChlorine", "18\nArgon", "19\nPotassium", "20\nCalcium",
        "21\nScandium", "22\nTitanium", "23\nVanadium", "24\nChromium", "25\nManganese",
        "26\nIron", "27\nCobalt", "28\nNickel", "29\nCopper", "30\nZinc",
        "31\nGallium", "32\nGermanium", "33\nArsenic", "34\nSelenium", "35\nBromine",
        "36\nKrypton", "37\nRubidium", "38\nStrontium", "39\nYttrium", "40\nZirconium",
        "41\nNiobium", "42\nMolybdenum", "43\nTechnetium", "44\nRuthenium", "45\nRhodium",
        "46\nPalladium", "47\nSilver", "48\nCadmium", "49\nIndium", "50\nTin",
        "51\nAntimony", "52\nTellurium", "53\nIodine", "54\nXenon", "55\nCaesium",
        "56\nBarium", "57\nLanthanum", "58\nCerium", "59\nPraseodymium", "60\nNeodymium",
        "61\nPromethium", "62\nSamarium", "63\nEuropium", "64\nGadolinium", "65\nTerbium",
        "66\nDysprosium", "67\nHolmium", "68\nErbium", "69\nThulium", "70\nYtterbium",
        "71\nLutetium", "72\nHafnium", "73\nTantalum", "74\nTungsten", "75\nRhenium",
        "76\nOsmium", "77\nIridium", "78\nPlatinum", "79\nGold", "80\nMercury",
        "81\nThallium", "82\nLead", "83\nBismuth", "84\nPolonium", "85\nAstatine",
        "86\nRadon", "87\nFrancium", "88\nRadium", "89\nActinium", "90\nThorium",
        "91\nProtactinium", "92\nUranium", "93\nNeptunium", "94\nPlutonium", "95\nAmericium",
        "96\nCurium", "97\nBerkelium", "98\nCalifornium", "99\nEinsteinium", "100\nFermium",
        "101\nMendelevium", "102\nNobelium", "103\nLawrencium", "104\nRutherfordium", "105\nDubnium",
        "106\nSeaborgium", "107\nBohrium", "108\nHassium", "109\nMeitnerium", "110\nDarmstadtium",
        "111\nRoentgenium", "112\nCopernicium", "113\nNihonium", "114\nFlerovium", "115\nMoscovium",
        "116\nLivermorium", "117\nTennessine", "118\nOganesson"
    };

    void Start()
    {
        GenerateElementButtons();
    }

    void GenerateElementButtons()
    {
        foreach (string name in elementNames)
        {

            GameObject newButton = Instantiate(buttonPrefab, contentParent);


            TMP_Text buttonText = newButton.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
            {
                buttonText.text = name;
            }

            newButton.name = "Button_" + name;
        }
    }
}