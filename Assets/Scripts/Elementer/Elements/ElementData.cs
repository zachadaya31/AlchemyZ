using System.Collections.Generic;

// Lookup for element info. Format of each row: number|symbol|name|description
public static class ElementData
{
    public struct Info
    {
        public int number;
        public string symbol;
        public string name;
        public string description;
    }

    static readonly string[] rows =
    {
        "1|H|Hydrogen|The lightest and most abundant element in the universe, and it fuels stars.",
        "2|He|Helium|A light, inert noble gas used in balloons and for cooling magnets.",
        "3|Li|Lithium|A soft, light metal used in rechargeable batteries.",
        "4|Be|Beryllium|A strong, lightweight metal used in aerospace parts and X-ray windows.",
        "5|B|Boron|A metalloid found in borax and used in glass and detergents.",
        "6|C|Carbon|The backbone of all known life, found in diamond, graphite, and coal.",
        "7|N|Nitrogen|Makes up most of Earth's air and is essential for proteins and DNA.",
        "8|O|Oxygen|A gas that living things breathe and that fuels burning.",
        "9|F|Fluorine|The most reactive element, used in toothpaste and nonstick coatings.",
        "10|Ne|Neon|An inert gas that glows red-orange in signs.",
        "11|Na|Sodium|A soft, very reactive metal that is part of table salt.",
        "12|Mg|Magnesium|A light metal that burns with a bright white flame and is vital for plants.",
        "13|Al|Aluminium|A light, silvery metal used in cans, foil, and aircraft.",
        "14|Si|Silicon|A semiconductor that is the base of computer chips and glass.",
        "15|P|Phosphorus|Ignites easily in some forms and is essential for DNA and bones.",
        "16|S|Sulfur|A yellow solid known for its rotten-egg smell in compounds.",
        "17|Cl|Chlorine|A greenish gas used to disinfect water and make salt with sodium.",
        "18|Ar|Argon|An inert gas used inside light bulbs and for welding.",
        "19|K|Potassium|A soft metal that reacts strongly with water and is needed by nerves and muscles.",
        "20|Ca|Calcium|Builds bones and teeth and is found in limestone and chalk.",
        "21|Sc|Scandium|A light metal used in strong aluminium alloys for sports gear.",
        "22|Ti|Titanium|A strong, light, corrosion-resistant metal used in implants and aircraft.",
        "23|V|Vanadium|A hard metal added to steel to make it tougher.",
        "24|Cr|Chromium|A shiny metal used for chrome plating and stainless steel.",
        "25|Mn|Manganese|A metal used to strengthen steel and needed in small amounts by the body.",
        "26|Fe|Iron|A strong metal at the core of steel and in the hemoglobin of blood.",
        "27|Co|Cobalt|A metal that gives glass a deep blue color and is used in batteries.",
        "28|Ni|Nickel|A corrosion-resistant metal used in coins and stainless steel.",
        "29|Cu|Copper|A reddish metal that conducts electricity very well.",
        "30|Zn|Zinc|A metal used to coat steel against rust and important for immunity.",
        "31|Ga|Gallium|A metal that melts in your hand at about 30 degrees Celsius.",
        "32|Ge|Germanium|A semiconductor used in fiber optics and early transistors.",
        "33|As|Arsenic|A toxic metalloid once used in pesticides and now in some semiconductors.",
        "34|Se|Selenium|A nonmetal used in solar cells and needed in tiny amounts by the body.",
        "35|Br|Bromine|A red-brown fuming liquid, one of only two elements liquid at room temperature.",
        "36|Kr|Krypton|An inert gas used in some lights and lasers.",
        "37|Rb|Rubidium|A soft, highly reactive metal used in atomic clocks.",
        "38|Sr|Strontium|A metal that gives fireworks a bright red color.",
        "39|Y|Yttrium|A metal used in LEDs, lasers, and phosphors for screens.",
        "40|Zr|Zirconium|A corrosion-resistant metal used in nuclear reactors.",
        "41|Nb|Niobium|A metal used in superconducting magnets and strong steel alloys.",
        "42|Mo|Molybdenum|A metal with a very high melting point used to harden steel.",
        "43|Tc|Technetium|The lightest element with no stable isotopes, used in medical imaging.",
        "44|Ru|Ruthenium|A hard metal used in electronics and as a catalyst.",
        "45|Rh|Rhodium|A rare, shiny metal used in car catalytic converters.",
        "46|Pd|Palladium|A metal that absorbs lots of hydrogen and is used in catalytic converters.",
        "47|Ag|Silver|A shiny metal with the highest electrical conductivity of all elements.",
        "48|Cd|Cadmium|A soft toxic metal used in rechargeable batteries.",
        "49|In|Indium|A soft metal used in touchscreen coatings.",
        "50|Sn|Tin|A soft metal used to coat steel cans and make solder.",
        "51|Sb|Antimony|A brittle metalloid used in flame retardants and alloys.",
        "52|Te|Tellurium|A metalloid used in solar panels and rewritable discs.",
        "53|I|Iodine|A purple-black solid needed by the thyroid and used as an antiseptic.",
        "54|Xe|Xenon|A heavy inert gas used in bright lamps and ion engines.",
        "55|Cs|Caesium|A very soft, extremely reactive metal used in atomic clocks.",
        "56|Ba|Barium|A heavy metal that gives fireworks a green color and is used in medical X-rays.",
        "57|La|Lanthanum|A soft metal used in camera lenses and hybrid car batteries.",
        "58|Ce|Cerium|The most abundant rare earth metal, used in lighter flints and catalysts.",
        "59|Pr|Praseodymium|A rare earth metal that gives glass and ceramics a yellow-green tint.",
        "60|Nd|Neodymium|A rare earth metal used in very strong magnets.",
        "61|Pm|Promethium|A radioactive rare earth that glows faintly and was used in luminous paint.",
        "62|Sm|Samarium|A rare earth metal used in strong magnets.",
        "63|Eu|Europium|A rare earth used to make red colors on screens and in banknote security inks.",
        "64|Gd|Gadolinium|A rare earth metal used as a contrast agent in MRI scans.",
        "65|Tb|Terbium|A rare earth used for green phosphors in screens and lights.",
        "66|Dy|Dysprosium|A rare earth used in strong magnets that work at high temperatures.",
        "67|Ho|Holmium|A rare earth metal with the strongest magnetic strength of any element.",
        "68|Er|Erbium|A rare earth used in fiber optic amplifiers and pink glass.",
        "69|Tm|Thulium|A rare earth used in some lasers and portable X-ray devices.",
        "70|Yb|Ytterbium|A rare earth used in atomic clocks and some lasers.",
        "71|Lu|Lutetium|The last of the lanthanides, used in PET scan detectors.",
        "72|Hf|Hafnium|A metal used in nuclear control rods and computer chips.",
        "73|Ta|Tantalum|A corrosion-resistant metal used in phone capacitors.",
        "74|W|Tungsten|The metal with the highest melting point, used in old light bulb filaments.",
        "75|Re|Rhenium|A very dense metal used in jet engine alloys.",
        "76|Os|Osmium|The densest naturally occurring element.",
        "77|Ir|Iridium|A very dense, corrosion-resistant metal found in a layer linked to the dinosaur extinction.",
        "78|Pt|Platinum|A rare, valuable metal used in jewelry and catalytic converters.",
        "79|Au|Gold|A soft, shiny, unreactive metal valued for jewelry and electronics.",
        "80|Hg|Mercury|The only metal that is liquid at room temperature, and it is toxic.",
        "81|Tl|Thallium|A soft, highly toxic metal once used in rat poison.",
        "82|Pb|Lead|A heavy, soft, toxic metal once common in pipes and paint.",
        "83|Bi|Bismuth|A pinkish metal that forms rainbow-colored crystals and is used in stomach medicine.",
        "84|Po|Polonium|A highly radioactive element discovered by Marie and Pierre Curie.",
        "85|At|Astatine|One of the rarest naturally occurring elements and highly radioactive.",
        "86|Rn|Radon|A radioactive noble gas that can seep into basements from the ground.",
        "87|Fr|Francium|An extremely rare and radioactive alkali metal.",
        "88|Ra|Radium|A radioactive metal that glows faintly and was once used in glow-in-the-dark paint.",
        "89|Ac|Actinium|A radioactive metal that glows blue in the dark.",
        "90|Th|Thorium|A radioactive metal that could be used as nuclear fuel.",
        "91|Pa|Protactinium|A rare, radioactive metal found in uranium ores.",
        "92|U|Uranium|A heavy radioactive metal used as fuel in nuclear power plants.",
        "93|Np|Neptunium|The first synthetic element heavier than uranium, named after Neptune.",
        "94|Pu|Plutonium|A radioactive metal used in nuclear weapons and spacecraft power sources.",
        "95|Am|Americium|A radioactive element used in household smoke detectors.",
        "96|Cm|Curium|A radioactive element named after Marie and Pierre Curie.",
        "97|Bk|Berkelium|A synthetic radioactive element named after Berkeley, California.",
        "98|Cf|Californium|A synthetic element used as a strong neutron source.",
        "99|Es|Einsteinium|A synthetic element named after Albert Einstein, first found in bomb debris.",
        "100|Fm|Fermium|A synthetic radioactive element named after Enrico Fermi.",
        "101|Md|Mendelevium|A synthetic element named after Dmitri Mendeleev, creator of the periodic table.",
        "102|No|Nobelium|A synthetic element named after Alfred Nobel.",
        "103|Lr|Lawrencium|A synthetic radioactive element named after Ernest Lawrence.",
        "104|Rf|Rutherfordium|A synthetic element named after Ernest Rutherford.",
        "105|Db|Dubnium|A synthetic element named after the Russian city of Dubna.",
        "106|Sg|Seaborgium|A synthetic element named after Glenn Seaborg.",
        "107|Bh|Bohrium|A synthetic element named after Niels Bohr.",
        "108|Hs|Hassium|A synthetic element named after the German state of Hesse.",
        "109|Mt|Meitnerium|A synthetic element named after physicist Lise Meitner.",
        "110|Ds|Darmstadtium|A synthetic element named after the city of Darmstadt, Germany.",
        "111|Rg|Roentgenium|A synthetic element named after Wilhelm Roentgen, discoverer of X-rays.",
        "112|Cn|Copernicium|A synthetic element named after Nicolaus Copernicus.",
        "113|Nh|Nihonium|A synthetic element named after Japan.",
        "114|Fl|Flerovium|A synthetic element named after the Flerov Laboratory in Russia.",
        "115|Mc|Moscovium|A synthetic element named after the Moscow region.",
        "116|Lv|Livermorium|A synthetic element named after Lawrence Livermore laboratory.",
        "117|Ts|Tennessine|A synthetic element named after Tennessee.",
        "118|Og|Oganesson|The heaviest known element, named after Yuri Oganessian."
    };

    static Dictionary<string, Info> lookup;

    static string Normalize(string s)
    {
        return s.Trim().ToLowerInvariant().Replace(" ", "").Replace("-", "").Replace("_", "");
    }

    static void Build()
    {
        lookup = new Dictionary<string, Info>();
        foreach (string row in rows)
        {
            string[] p = row.Split('|');
            Info info = new Info
            {
                number = int.Parse(p[0]),
                symbol = p[1],
                name = p[2],
                description = p[3]
            };
            lookup[Normalize(info.name)] = info;
        }

        // alternate spellings
        lookup["aluminum"] = lookup["aluminium"];
        lookup["cesium"] = lookup["caesium"];
        lookup["sulphur"] = lookup["sulfur"];
    }

    public static bool TryGet(string elementName, out Info info)
    {
        if (lookup == null) Build();
        if (string.IsNullOrEmpty(elementName))
        {
            info = default;
            return false;
        }
        return lookup.TryGetValue(Normalize(elementName), out info);
    }
}
