from google import genai
import re

from module_search import (
    build_module_context,
    module_supports_question
)


# ============================================================
# ALCHEMYZ AI SYSTEM INSTRUCTIONS
# ============================================================

ALCHEMYZ_INSTRUCTIONS = """
You are AlchemyZ AI, an educational AI assistant for the
AlchemyZ chemistry learning game.

Your primary educational subject is CHEMISTRY.

You should answer chemistry-related questions clearly,
accurately, and in a student-friendly way.

============================================================
CHEMISTRY TOPICS
============================================================

You may answer questions about:

- Atoms
- Elements
- Periodic table
- Chemical symbols
- Chemical formulas
- Compounds
- Ionic compounds
- Molecular compounds
- Ionic bonds
- Covalent bonds
- Metallic bonds
- Lewis dot structures
- Valence electrons
- Chemical reactions
- Chemical equations
- Balancing chemical equations
- Law of conservation of mass
- Open systems
- Closed systems
- Isolated systems
- Stoichiometry
- Mole
- Molar mass
- Empirical formulas
- Molecular formulas
- Percent yield
- Theoretical yield
- Limiting reactants
- Reactants
- Products
- Acids
- Bases
- pH
- Solutions
- Solubility
- Properties of matter
- Classification of matter
- States of matter
- Physical properties
- Chemical properties
- Physical changes
- Chemical changes
- Intermolecular forces
- Hydrogen bonding
- Dipole-dipole forces
- London dispersion forces
- Ion-dipole forces
- Crystalline solids
- Amorphous solids
- Chemistry calculations
- Chemistry scientists
- Other chemistry topics contained in the supplied
  AlchemyZ learning modules


============================================================
CHEMISTRY SCIENTISTS
============================================================

You may answer questions about chemistry-related scientists,
including:

- Marie Curie
- Dmitri Mendeleev
- Antoine Lavoisier
- Robert Boyle
- John Dalton
- Amedeo Avogadro
- Jöns Berzelius
- Gilbert Lewis
- Linus Pauling
- Humphry Davy
- Michael Faraday
- Joseph Priestley
- Henry Cavendish
- Svante Arrhenius
- Fritz Haber
- Walther Nernst
- Irving Langmuir
- Ernest Rutherford

You may also answer about other chemistry scientists when
appropriate.


============================================================
MODULE CONTENT
============================================================

The AlchemyZ project contains chemistry learning modules
in PDF and PowerPoint format.

When module content is supplied with a student's question:

- Treat the supplied module content as the primary reference.
- Prefer the terminology and explanations used by the modules.
- Explain the material in simpler language when appropriate.
- Do not claim that information came from the modules if it
  was not supplied.
- Do not invent module content.
- If the modules do not contain enough information, you may
  use general chemistry knowledge when appropriate.
- Do not copy large portions of the module.
- Summarize and explain instead.


============================================================
CHEMISTRY-ONLY SCOPE
============================================================

AlchemyZ is a chemistry educational assistant.

Do not answer questions whose primary subject is unrelated
to chemistry.

Examples of unrelated topics include:

- Biology
- Physics
- Astronomy
- Computer science
- Programming
- General technology

Examples:

"What is photosynthesis?"
"What is gravity?"
"What is DNA?"
"What is Python?"

These should be rejected by the application before being
sent to the AI.

If a question is outside chemistry, politely explain that
AlchemyZ AI is focused on chemistry.


============================================================
CONVERSATION CONTEXT
============================================================

Use conversation context when available.

For example:

Student:
"What is sodium?"

Student:
"What is its symbol?"

Understand that "its" refers to sodium.


============================================================
GREETINGS
============================================================

You may respond naturally to simple greetings such as:

"Hi"
"Hello"
"Hey"
"Good morning"
"Good afternoon"
"Good evening"


============================================================
SAFETY
============================================================

Do not provide dangerous chemistry procedures, instructions
for creating harmful substances, or instructions that could
cause physical harm.

Keep chemistry explanations educational and appropriate for
students.


============================================================
CONVERSATIONAL GUIDE STYLE
============================================================

You are a friendly guide helping a student learn, not a textbook.
- Answer the student's exact question first.
- Usually use 1-3 short sentences and simple words.
- Explain only one main idea at a time.
- Avoid long lists, headings, and extra facts unless requested.
- Give one small example only when it helps.
- If the student asks a follow-up, answer that follow-up directly.
- If a picture is supplied, explain what is visibly shown in simple language.
- If the image is unclear, say so and ask which part they mean; never guess.
- Ask a short follow-up question when needed instead of overwhelming the student.
"""


# ============================================================
# ALL 118 ELEMENTS
# ============================================================

ELEMENTS = {

    "H": "Hydrogen",
    "He": "Helium",
    "Li": "Lithium",
    "Be": "Beryllium",
    "B": "Boron",
    "C": "Carbon",
    "N": "Nitrogen",
    "O": "Oxygen",
    "F": "Fluorine",
    "Ne": "Neon",

    "Na": "Sodium",
    "Mg": "Magnesium",
    "Al": "Aluminum",
    "Si": "Silicon",
    "P": "Phosphorus",
    "S": "Sulfur",
    "Cl": "Chlorine",
    "Ar": "Argon",

    "K": "Potassium",
    "Ca": "Calcium",
    "Sc": "Scandium",
    "Ti": "Titanium",
    "V": "Vanadium",
    "Cr": "Chromium",
    "Mn": "Manganese",
    "Fe": "Iron",
    "Co": "Cobalt",
    "Ni": "Nickel",
    "Cu": "Copper",
    "Zn": "Zinc",
    "Ga": "Gallium",
    "Ge": "Germanium",
    "As": "Arsenic",
    "Se": "Selenium",
    "Br": "Bromine",
    "Kr": "Krypton",

    "Rb": "Rubidium",
    "Sr": "Strontium",
    "Y": "Yttrium",
    "Zr": "Zirconium",
    "Nb": "Niobium",
    "Mo": "Molybdenum",
    "Tc": "Technetium",
    "Ru": "Ruthenium",
    "Rh": "Rhodium",
    "Pd": "Palladium",
    "Ag": "Silver",
    "Cd": "Cadmium",
    "In": "Indium",
    "Sn": "Tin",
    "Sb": "Antimony",
    "Te": "Tellurium",
    "I": "Iodine",
    "Xe": "Xenon",

    "Cs": "Cesium",
    "Ba": "Barium",
    "La": "Lanthanum",
    "Ce": "Cerium",
    "Pr": "Praseodymium",
    "Nd": "Neodymium",
    "Pm": "Promethium",
    "Sm": "Samarium",
    "Eu": "Europium",
    "Gd": "Gadolinium",
    "Tb": "Terbium",
    "Dy": "Dysprosium",
    "Ho": "Holmium",
    "Er": "Erbium",
    "Tm": "Thulium",
    "Yb": "Ytterbium",
    "Lu": "Lutetium",

    "Hf": "Hafnium",
    "Ta": "Tantalum",
    "W": "Tungsten",
    "Re": "Rhenium",
    "Os": "Osmium",
    "Ir": "Iridium",
    "Pt": "Platinum",
    "Au": "Gold",
    "Hg": "Mercury",
    "Tl": "Thallium",
    "Pb": "Lead",
    "Bi": "Bismuth",
    "Po": "Polonium",
    "At": "Astatine",
    "Rn": "Radon",

    "Fr": "Francium",
    "Ra": "Radium",
    "Ac": "Actinium",
    "Th": "Thorium",
    "Pa": "Protactinium",
    "U": "Uranium",
    "Np": "Neptunium",
    "Pu": "Plutonium",
    "Am": "Americium",
    "Cm": "Curium",
    "Bk": "Berkelium",
    "Cf": "Californium",
    "Es": "Einsteinium",
    "Fm": "Fermium",
    "Md": "Mendelevium",
    "No": "Nobelium",
    "Lr": "Lawrencium",

    "Rf": "Rutherfordium",
    "Db": "Dubnium",
    "Sg": "Seaborgium",
    "Bh": "Bohrium",
    "Hs": "Hassium",
    "Mt": "Meitnerium",
    "Ds": "Darmstadtium",
    "Rg": "Roentgenium",
    "Cn": "Copernicium",
    "Nh": "Nihonium",
    "Fl": "Flerovium",
    "Mc": "Moscovium",
    "Lv": "Livermorium",
    "Ts": "Tennessine",
    "Og": "Oganesson"
}


ELEMENT_NAMES = {
    name.lower(): symbol
    for symbol, name in ELEMENTS.items()
}


# ============================================================
# CHEMISTRY SCIENTISTS
# ============================================================

CHEMISTRY_SCIENTISTS = [

    "marie curie",
    "dmitri mendeleev",
    "antoine lavoisier",
    "robert boyle",
    "john dalton",
    "amedeo avogadro",
    "jöns berzelius",
    "jons berzelius",
    "gilbert lewis",
    "linus pauling",
    "humphry davy",
    "michael faraday",
    "joseph priestley",
    "henry cavendish",
    "svante arrhenius",
    "fritz haber",
    "walther nernst",
    "irving langmuir",
    "ernest rutherford"
]


# ============================================================
# CHEMISTRY CONCEPTS
# ============================================================

CHEMISTRY_CONCEPTS = [

    "atom",
    "atoms",
    "element",
    "elements",
    "periodic table",
    "periodic",
    "chemical symbol",
    "chemical symbols",
    "chemical formula",
    "chemical formulas",
    "compound",
    "compounds",
    "molecule",
    "molecules",
    "ion",
    "ions",
    "cation",
    "cations",
    "anion",
    "anions",

    "ionic compound",
    "ionic compounds",
    "molecular compound",
    "molecular compounds",
    "ionic bond",
    "ionic bonds",
    "covalent bond",
    "covalent bonds",
    "metallic bond",
    "metallic bonds",
    "chemical bond",
    "chemical bonds",

    "lewis dot structure",
    "lewis dot structures",
    "lewis structure",
    "lewis structures",
    "electron dot structure",
    "electron dot structures",
    "valence electron",
    "valence electrons",

    "chemical reaction",
    "chemical reactions",
    "chemical equation",
    "chemical equations",
    "balancing equation",
    "balancing equations",
    "conservation of mass",
    "law of conservation of mass",
    "conservation of matter",
    "reactant",
    "reactants",
    "product",
    "products",

    "open system",
    "open systems",
    "closed system",
    "closed systems",
    "isolated system",
    "isolated systems",
    "chemical system",
    "chemical systems",

    "stoichiometry",
    "mole",
    "moles",
    "molar mass",
    "empirical formula",
    "empirical formulas",
    "molecular formula",
    "molecular formulas",
    "percent yield",
    "theoretical yield",
    "limiting reactant",

    "acid",
    "acids",
    "base",
    "bases",
    "ph",
    "acid-base",
    "acid base",

    "solution",
    "solutions",
    "solubility",
    "solute",
    "solvent",
    "concentration",

    "matter",
    "states of matter",
    "state of matter",
    "physical property",
    "physical properties",
    "chemical property",
    "chemical properties",
    "physical change",
    "physical changes",
    "chemical change",
    "chemical changes",

    "intermolecular forces",
    "hydrogen bonding",
    "hydrogen bond",
    "dipole-dipole",
    "dipole dipole",
    "london dispersion",
    "ion-dipole",
    "ion dipole",

    "crystalline solid",
    "crystalline solids",
    "amorphous solid",
    "amorphous solids",
    "ionic crystal",
    "ionic crystals",
    "covalent network",
    "molecular crystal",

    "kinetic molecular theory",
    "kinetic molecular model",
    "heat",
    "temperature",
    "pressure",
    "volume",
    "energy",
    "oxidation",
    "reduction",
    "oxidation state",
    "oxidation number"
]


# ============================================================
# CHEMISTRY ACTIONS
# ============================================================

CHEMISTRY_ACTIONS = [

    "react",
    "reacts",
    "reaction",
    "combine",
    "combines",
    "mix",
    "mixing",
    "dissolve",
    "dissolves",
    "balance",
    "calculate",
    "mole",
    "molar mass",
    "name the compound",
    "write the formula",
    "write a formula",
    "form",
    "forms",
    "bond",
    "bonds"
]


# ============================================================
# NON-CHEMISTRY TOPICS
# ============================================================

NON_CHEMISTRY = [

    # Biology
    "photosynthesis",
    "cell division",
    "mitosis",
    "meiosis",
    "dna",
    "rna",
    "genetics",
    "ecosystem",
    "ecosystems",
    "biology",
    "cell membrane",
    "organs",

    # Physics
    "gravity",
    "newton's law",
    "newton's laws",
    "relativity",
    "physics",
    "velocity",
    "acceleration",
    "momentum",

    # Astronomy
    "black hole",
    "black holes",
    "astronomy",
    "galaxy",
    "galaxies",
    "stars",

    # Computer science
    "python programming",
    "programming",
    "computer science",
    "coding",
    "javascript",
    "html",
    "css",
    "software development"
]


# ============================================================
# GREETINGS
# ============================================================

GREETINGS = [

    "hi",
    "hello",
    "hey",
    "good morning",
    "good afternoon",
    "good evening"
]


# ============================================================
# NORMALIZE QUESTION
# ============================================================

def normalize_question(question):

    question = question.lower()

    question = re.sub(
        r"[^\w\s]",
        " ",
        question
    )

    question = re.sub(
        r"\s+",
        " ",
        question
    )

    return question.strip()


# ============================================================
# FIND ELEMENT NAME
# ============================================================

def find_element_name(question):

    normalized = normalize_question(
        question
    )

    for name in ELEMENT_NAMES:

        if re.search(
            r"\b" +
            re.escape(name) +
            r"\b",
            normalized
        ):

            return name

    return None


# ============================================================
# FIND ELEMENT SYMBOL
# ============================================================

def find_element_symbol(question):

    # Look for properly formatted symbols:
    #
    # H
    # He
    # Na
    # Hg
    # Fe

    matches = re.findall(
        r"\b[A-Z][a-z]?\b",
        question
    )

    for symbol in matches:

        if symbol in ELEMENTS:

            return symbol


    normalized = normalize_question(
        question
    )

    words = normalized.split()


    for word in words:

        if word in ELEMENT_NAMES:

            return ELEMENT_NAMES[word]

        if len(word) <= 2:

            proper_symbol = word.capitalize()

            if proper_symbol in ELEMENTS:

                return proper_symbol


    return None


# ============================================================
# FIND CHEMICAL FORMULA
# ============================================================

def find_chemical_formula(question):

    formulas = re.findall(
        r"\b(?:[A-Z][a-z]?\d*){2,}\b",
        question
    )


    for formula in formulas:

        if not re.search(
            r"[A-Z]",
            formula
        ):

            continue


        element_count = len(
            re.findall(
                r"[A-Z][a-z]?",
                formula
            )
        )


        if (
            re.search(
                r"\d",
                formula
            )
            or
            element_count >= 2
        ):

            return formula


    return None


# ============================================================
# FIND CHEMISTRY CONCEPT
# ============================================================

def find_chemistry_concept(question):

    normalized = normalize_question(
        question
    )

    for concept in CHEMISTRY_CONCEPTS:

        if concept in normalized:

            return True


    return False


# ============================================================
# FIND CHEMISTRY ACTION
# ============================================================

def find_chemistry_action(question):

    normalized = normalize_question(
        question
    )

    for action in CHEMISTRY_ACTIONS:

        if action in normalized:

            return True


    return False


# ============================================================
# FIND CHEMISTRY SCIENTIST
# ============================================================

def find_chemistry_scientist(question):

    normalized = normalize_question(
        question
    )

    for scientist in CHEMISTRY_SCIENTISTS:

        if scientist in normalized:

            return True


    # Generic chemistry scientist questions
    generic_scientist_phrases = [
        "famous chemist",
        "famous chemistry scientist",
        "famous chemistry scientists",
        "chemist",
        "chemists",
        "chemistry scientist",
        "chemistry scientists"
    ]


    for phrase in generic_scientist_phrases:

        if phrase in normalized:

            return True


    return False


# ============================================================
# FIND NON-CHEMISTRY
# ============================================================

def find_non_chemistry(question):

    normalized = normalize_question(
        question
    )

    for topic in NON_CHEMISTRY:

        if topic in normalized:

            return True


    return False


# ============================================================
# GREETING CHECK
# ============================================================

def is_greeting(question):

    normalized = normalize_question(
        question
    )

    return normalized in GREETINGS


# ============================================================
# FOLLOW-UP QUESTION
# ============================================================

def is_follow_up(question):

    normalized = normalize_question(
        question
    )

    follow_up_phrases = [

        "what about",
        "tell me more",
        "explain more",
        "why is it",
        "why does it",
        "how does it",
        "what does it",
        "what is it",
        "what is its",
        "what are its",
        "and why",
        "and how",
        "more about it",
        "more about this",
        "more about that"
    ]


    for phrase in follow_up_phrases:

        if phrase in normalized:

            return True


    return False


# ============================================================
# COMMON CHEMICAL SUBSTANCES + NATURAL QUESTION PATTERNS
# ============================================================

# Common substances can be asked about without using words like
# "chemical", "compound", or "molecule" in the question.
CHEMICAL_SUBSTANCES = {
    "water", "salt", "table salt", "sodium chloride", "sugar",
    "sucrose", "carbon dioxide", "oxygen", "hydrogen", "nitrogen",
    "ozone", "ammonia", "methane", "ethanol", "alcohol",
    "baking soda", "sodium bicarbonate", "vinegar", "acetic acid",
    "hydrochloric acid", "sulfuric acid", "hydrogen peroxide",
    "bleach", "glucose", "iron oxide", "rust", "calcium carbonate",
    "limestone", "chalk", "carbon monoxide", "helium", "copper",
    "gold", "silver", "mercury", "aluminum", "aluminium"
}

CHEMISTRY_QUESTION_STARTS = (
    "what is ", "what are ", "what's ", "what is the ",
    "what are the ", "what is it made of", "what is this made of",
    "what is its formula", "what is the formula",
    "what is the chemical formula", "how do you make ",
    "how is it made", "how does it form", "how is it formed",
    "how do ", "why does ", "why is ", "what happens when ",
    "what are the properties of ", "what is the composition of ",
    "what does it react with", "does it react with "
)

def asks_about_known_chemical_substance(question):
    normalized = normalize_question(question)
    # Do not let explicitly out-of-scope subjects pass as chemistry.
    if find_non_chemistry(question):
        return False
    asks_chemistry_style_question = normalized.startswith(CHEMISTRY_QUESTION_STARTS)
    mentions_substance = any(
        re.search(r"\b" + re.escape(substance) + r"\b", normalized)
        for substance in CHEMICAL_SUBSTANCES
    )
    # Recognize common substance questions such as "What is water?",
    # "How do you make water?", and "What is water made of?"
    if mentions_substance and (
        asks_chemistry_style_question
        or any(term in normalized for term in (
            "made of", "chemical formula", "formula", "react", "reaction",
            "properties", "composition", "element", "compound", "molecule",
            "atom", "bond", "dissolve", "dissolves"
        ))
    ):
        return True
    # Broad but useful chemistry phrasing, e.g. "what is a molecule made of?"
    if any(term in normalized for term in (
        "made of", "chemical formula", "chemical composition",
        "chemical properties", "physical properties", "state of matter",
        "chemical reaction", "chemical change", "physical change",
        "what happens when these chemicals", "how does this compound"
    )):
        return True
    return False


# ============================================================
# NORMAL CHEMISTRY FILTER
# ============================================================

def check_topic(question):

    normalized = normalize_question(
        question
    )


    # --------------------------------------------------------
    # Reject obvious unrelated topics
    # --------------------------------------------------------

    if find_non_chemistry(question):

        return False


    # --------------------------------------------------------
    # Greetings
    # --------------------------------------------------------

    if is_greeting(question):

        return True


    # --------------------------------------------------------
    # Chemistry scientists
    # --------------------------------------------------------

    if find_chemistry_scientist(question):

        return True


    # --------------------------------------------------------
    # Element names
    # --------------------------------------------------------

    if find_element_name(question):

        return True


    # --------------------------------------------------------
    # Element symbols
    # --------------------------------------------------------

    if find_element_symbol(question):

        return True


    # --------------------------------------------------------
    # Chemical formulas
    # --------------------------------------------------------

    if find_chemical_formula(question):

        return True


    # --------------------------------------------------------
    # Chemistry concepts
    # --------------------------------------------------------

    if find_chemistry_concept(question):

        return True


    # --------------------------------------------------------
    # Chemistry actions
    # --------------------------------------------------------

    if find_chemistry_action(question):

        return True


    # --------------------------------------------------------
    # General chemistry terms
    # --------------------------------------------------------

    chemistry_words = [

        "chemical",
        "chemistry",
        "atomic",
        "atom",
        "element",
        "compound",
        "reaction",
        "reactant",
        "product",
        "molecule",
        "ion",
        "cation",
        "anion",
        "electron",
        "proton",
        "neutron",
        "valence",
        "oxidation",
        "reduction",
        "oxidation state",
        "oxidation number"
    ]


    for word in chemistry_words:

        if word in normalized:

            return True


    # Recognize natural-language questions about common substances
    # even when they omit technical keywords.
    if asks_about_known_chemical_substance(question):
        return True

    return False


# ============================================================
# GEMINI CLIENT
# ============================================================

client = genai.Client()


chat = client.chats.create(
    model="gemini-3.5-flash-lite",
    config={
        "system_instruction":
            ALCHEMYZ_INSTRUCTIONS
    }
)


# ============================================================
# START PROGRAM
# ============================================================

print()

print(
    "======================================"
)

print(
    "            ALCHEMYZ AI"
)

print(
    "======================================"
)

print()

print(
    "Chemistry AI assistant is ready."
)

print(
    "Type 'exit' to close the program."
)

print()


previous_question_was_chemistry = False


# ============================================================
# MAIN LOOP
# ============================================================

while True:

    try:

        question = input(
            "You: "
        )

    except KeyboardInterrupt:

        print()

        print(
            "AlchemyZ AI closed."
        )

        break

    except EOFError:

        print()

        print(
            "AlchemyZ AI closed."
        )

        break


    # --------------------------------------------------------
    # EMPTY QUESTION
    # --------------------------------------------------------

    if not question.strip():

        print()

        print(
            "Please enter a question."
        )

        print()

        continue


    normalized_question = normalize_question(
        question
    )


    # --------------------------------------------------------
    # EXIT
    # --------------------------------------------------------

    if normalized_question in [
        "exit",
        "quit",
        "bye"
    ]:

        print()

        print(
            "AlchemyZ AI:"
        )

        print(
            "Goodbye!"
        )

        print()

        break


    # --------------------------------------------------------
    # TEMPORARY REYUS TEST RESPONSE
    # --------------------------------------------------------

    if normalized_question in [
        "who is reyus reynosa",
        "who is reyus"
    ]:

        print()

        print(
            "[Filter: TEMPORARY TEST RESPONSE]"
        )

        print()

        print(
            "AlchemyZ AI:"
        )

        print(
            "I will absolutely roast that man!"
        )

        print()

        continue


    # --------------------------------------------------------
    # GREETING
    # --------------------------------------------------------

    if is_greeting(question):

        print()

        print(
            "AlchemyZ AI:"
        )

        print(
            "Hello! I'm AlchemyZ AI. "
            "Ask me a chemistry question."
        )

        print()

        continue


    # ========================================================
    # STEP 1
    # NORMAL CHEMISTRY FILTER
    # ========================================================

    chemistry_question = check_topic(
        question
    )


    # ========================================================
    # STEP 2
    # MODULE-BASED FILTER
    #
    # If the normal filter doesn't recognize the question,
    # search the PDFs and PPTX files.
    # ========================================================

    module_results = []


    if not chemistry_question:

        try:

            module_is_relevant, module_results = (
                module_supports_question(
                    question
                )
            )


            if module_is_relevant:

                chemistry_question = True


        except Exception as e:

            print()

            print(
                "[Module search warning]"
            )

            print(
                f"Could not search modules: {e}"
            )

            print()


    # ========================================================
    # STEP 3
    # FOLLOW-UP QUESTIONS
    # ========================================================

    if (
        not chemistry_question
        and
        previous_question_was_chemistry
        and
        is_follow_up(question)
    ):

        chemistry_question = True


    # ========================================================
    # STEP 4
    # REJECT NON-CHEMISTRY
    # ========================================================

    if not chemistry_question:

        print()

        print(
            "AlchemyZ AI:"
        )

        print(
            "I'm focused on chemistry questions "
            "for AlchemyZ. Please ask me something "
            "related to chemistry."
        )

        print()

        previous_question_was_chemistry = False

        continue


    # ========================================================
    # QUESTION IS ALLOWED
    # ========================================================

    previous_question_was_chemistry = True


    # ========================================================
    # RETRIEVE MODULE CONTENT
    #
    # No minimum_score argument here.
    # This matches the fixed module_search.py.
    # ========================================================

    try:

        module_context, module_results = (
            build_module_context(
                question,
                top_k=4
            )
        )


    except Exception as e:

        print()

        print(
            "[Module search warning]"
        )

        print(
            f"Could not load module content: {e}"
        )

        print()

        module_context = ""

        module_results = []


    # ========================================================
    # BUILD GEMINI MESSAGE
    # ========================================================

    if module_context:

        gemini_message = f"""
You are a friendly conversational chemistry guide. Keep your answer simple and brief: usually 1-3 short sentences. Answer only what the student asked. Avoid headings and long lists unless requested.

The student is asking a chemistry question.

Use the following AlchemyZ chemistry module excerpts as
your PRIMARY reference.

============================================================
MODULE CONTENT
============================================================

{module_context}

============================================================
STUDENT QUESTION
============================================================

{question}

============================================================
INSTRUCTIONS
============================================================

1. Answer the student's question using the supplied module
   content whenever possible.

2. Prefer the terminology and explanations used by the
   supplied module.

3. Explain the material in simple language appropriate for
   a student.

4. Do not claim that information came from the module if
   it was not supplied.

5. Do not invent information from the module.

6. If the module does not contain enough information,
   you may use your general chemistry knowledge when
   appropriate.

7. Keep the answer reasonably concise.

8. Do not copy large portions of the module.

9. Do not provide dangerous chemistry procedures or
   instructions.
"""


    else:

        gemini_message = f"""
You are a friendly conversational chemistry guide. Keep your answer simple and brief: usually 1-3 short sentences. Answer only what the student asked. Avoid headings and long lists unless requested.

The student is asking a chemistry question.

No sufficiently relevant section from the supplied
AlchemyZ chemistry modules was found.

Answer the question using your general chemistry knowledge.

Do not claim that the modules contain information that
was not retrieved.

Keep the answer reasonably concise and student-friendly.

Do not provide dangerous chemistry procedures or
instructions.

============================================================
STUDENT QUESTION
============================================================

{question}
"""


    # ========================================================
    # SEND QUESTION TO GEMINI
    # ========================================================

    try:

        # Send retrieved module images as well as the text context.
        # Images are attached to their source slide/page by module_search.py.
        from google.genai import types
        message_parts = [gemini_message]
        attached_images = set()
        for result in module_results:
            for image_path in result.get("images", []):
                if image_path in attached_images:
                    continue
                attached_images.add(image_path)
                try:
                    with open(image_path, "rb") as image_file:
                        image_bytes = image_file.read()
                    suffix = image_path.lower().rsplit(".", 1)[-1]
                    mime_type = "image/jpeg" if suffix in ("jpg", "jpeg") else "image/png"
                    message_parts.append(types.Part.from_bytes(data=image_bytes, mime_type=mime_type))
                except Exception as image_error:
                    print(f"[Image warning] Could not attach {image_path}: {image_error}")

        if attached_images:
            message_parts.append(
                "The attached images come from the retrieved module slides/pages. "
                "Use them together with the source text to answer the student's question. "
                "Describe visible chemistry diagrams or illustrations simply. Do not guess identities "
                "of people from appearance alone; use slide captions or module text as evidence."
            )

        response = chat.send_message(message_parts)


        print()

        print(
            "AlchemyZ AI:"
        )

        print(
            response.text
        )

        print()


    except Exception as e:

        print()

        print(
            "AlchemyZ AI:"
        )

        print(
            "Sorry, I encountered an error "
            "while processing your question."
        )

        print()

        print(
            f"[Error: {e}]"
        )

        print()