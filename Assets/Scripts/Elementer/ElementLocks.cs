using System;
using System.Collections.Generic;
using UnityEngine;

// Which elements are LOCKED in the Elementer (the "corrupted database").
// - baseUnlocked: always available (the elements the story mixes with)
// - groups[0] unlocks when Chapter 1 is finished, groups[1] when Chapter 2 is finished, ...
// - anything else stays locked until Chapter 10 is finished (then everything is unlocked)
// Saved per student with PlayerPrefs, so no database changes needed.
public static class ElementLocks
{
    static readonly HashSet<string> baseUnlocked = new HashSet<string>
    {
        "Hydrogen", "Carbon", "Nitrogen", "Oxygen", "Sodium",
        "Magnesium", "Silicon", "Sulfur", "Chlorine", "Calcium"
    };

    // each chapter's repair material is the first one in its group
    static readonly string[][] groups =
    {
        new[] { "Helium", "Lithium", "Beryllium", "Boron", "Fluorine", "Neon" },                       // after Chapter 1
        new[] { "Iron", "Aluminium", "Phosphorus", "Argon", "Scandium", "Titanium" },                  // after Chapter 2
        new[] { "Copper", "Potassium", "Vanadium", "Chromium", "Manganese", "Cobalt" },                // after Chapter 3
        new[] { "Silver", "Gallium", "Germanium", "Arsenic", "Selenium", "Bromine" },                  // after Chapter 4
        new[] { "Tin", "Krypton", "Rubidium", "Strontium", "Yttrium", "Zirconium" },                   // after Chapter 5
        new[] { "Lead", "Niobium", "Molybdenum", "Ruthenium", "Rhodium", "Palladium" },                // after Chapter 6
        new[] { "Gold", "Cadmium", "Indium", "Antimony", "Tellurium", "Iodine" },                      // after Chapter 7
        new[] { "Zinc", "Xenon", "Caesium", "Barium", "Lanthanum", "Cerium" },                         // after Chapter 8
        new[] { "Nickel", "Tungsten", "Osmium", "Iridium", "Mercury", "Bismuth" },                     // after Chapter 9
    };

    private static string Key()
    {
        return "ChaptersDone_" + SessionManager.Instance.studentID;
    }

    public static int ChaptersDone()
    {
        return PlayerPrefs.GetInt(Key(), 0);
    }

    public static bool IsLocked(string elementName)
    {
        if (baseUnlocked.Contains(elementName)) return false;

        int done = ChaptersDone();
        if (done >= 10) return false;

        for (int c = 0; c < groups.Length; c++)
        {
            if (Array.IndexOf(groups[c], elementName) >= 0)
                return done < c + 1; // unlocked once chapter c+1 is finished
        }
        return true; // not in any group: locked until the end of the game
    }

    // call this at the END of a chapter (after the recalibration). It saves the unlock
    // and returns the line for Clarence AI to say, ex: AiSays("...", ElementLocks.Unlock(3))
    public static string Unlock(int chapter)
    {
        if (chapter > ChaptersDone())
        {
            PlayerPrefs.SetInt(Key(), chapter);
            PlayerPrefs.Save();
        }

        if (chapter >= 10) return "ALL ELEMENTS RESTORED.";
        return "ELEMENTS UNLOCKED: " + string.Join(", ", groups[chapter - 1]) + ".";
    }
}
