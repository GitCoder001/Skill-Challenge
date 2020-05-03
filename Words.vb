Module Words
	' module generates words
	Dim WordRand As New Random

	Function Get3Letter() As String
        Dim ThreeLetterWords As New List(Of String)({"baa", "ace", "act", "add", "aft", "aga", "age", "ago", "aha", "aid", "ail", "aim", "air", "ale", "all", "alp", "alt", "amp", "and", "ant",
        "any", "ape", "app", "apt", "arc", "are", "ark", "arm", "art", "ask", "asp", "ate", "awn", "axe", "bad", "bag", "bam", "ban", "bap", "bar",
        "bat", "bay", "bed", "bee", "beg", "bet", "bib", "bid", "big", "bin", "bio", "bit", "biz", "bob", "bod", "bog", "boo", "bot", "bow", "box",
        "boy", "bra", "bud", "bug", "bum", "bun", "bus", "but", "buy", "bye", "cab", "cad", "cam", "can", "cap", "car", "cat", "cob", "cod", "cog",
        "col", "con", "cop", "cos", "cot", "cow", "cox", "coy", "coz", "cry", "cub", "cue", "cup", "cut", "dan", "day", "dee", "did", "die", "dig",
        "dim", "din", "dip", "dis", "doc", "dog", "dom", "dot", "dry", "dub", "dud", "due", "dug", "dum", "eat", "eau", "ebb", "eco", "eek", "eel",
        "egg", "ego", "elf", "elk", "elm", "eon", "era", "fab", "fad", "fag", "fan", "far", "fat", "fax", "fed", "fee", "fen", "few", "fez", "fib",
        "fig", "fin", "fir", "fit", "fix", "flu", "fly", "fob", "foe", "fog", "for", "fox", "fro", "fry", "fug", "fun", "gap", "gas", "gee", "gel",
        "gem", "get", "gig", "gin", "git", "gum", "gun", "gut", "guy", "had", "hag", "ham", "has", "hat", "hay", "hem", "hen", "her", "hex", "hey",
        "hid", "his", "hit", "hob", "hog", "hop", "hot", "how", "hub", "hue", "hug", "hum", "hut", "ice", "icy", "ilk", "ill", "imp", "ink", "inn",
        "ion", "ire", "irk", "its", "ivy", "jab", "jag", "jam", "jar", "jaw", "jet", "jig", "job", "jog", "jot", "joy", "jug", "keg", "key", "kid",
        "kin", "kip", "kit", "koi", "kop", "lad", "lag", "lap", "law", "lax", "lay", "lea", "led", "leg", "let", "ley", "lid", "lie", "lip", "lob",
        "log", "loo", "lop", "lot", "low", "lug", "lux", "mad", "man", "map", "mat", "max", "may", "men", "mew", "mid", "mix", "mod", "moi", "mom",
        "moo", "mop", "mow", "mud", "nab", "nag", "nap", "net", "new", "nil", "nit", "nix", "nod", "nor", "not", "now", "nub", "nun", "nut", "oak",
        "oar", "oat", "off", "oik", "oil", "old", "one", "opt", "ore", "our", "out", "own", "oxy", "pad", "pal", "pan", "pat", "pay", "pea", "pee",
        "peg", "pen", "per", "pet", "pie", "pig", "pin", "pip", "pit", "ply", "pod", "pop", "pot", "pow", "pox", "pro", "pry", "pub", "pud", "pug",
        "pup", "pur", "put", "rad", "rag", "ram", "ran", "rap", "rat", "raw", "ray", "red", "rid", "rig", "rim", "rip", "rob", "rod", "roe", "rot",
        "row", "rub", "rug", "rum", "run", "rut", "sac", "sad", "sag", "sap", "sat", "sax", "say", "sea", "sec", "see", "set", "she", "sin", "sip",
        "sir", "sit", "six", "sky", "sly", "sob", "son", "soy", "spy", "sty", "sub", "sue", "sum", "sun", "sus", "tab", "tad", "tag", "tan", "tap",
        "tar", "tax", "taa", "tee", "ten", "the", "tho", "thy", "tie", "tin", "tip", "toe", "tog", "too", "top", "tot", "tow", "toy", "try", "tub",
        "tug", "tum", "tut", "tux", "ugh", "umm", "uni", "urn", "use", "vac", "van", "vat", "veg", "vet", "vex", "vow", "vum", "wad", "wag", "war",
        "was", "wax", "way", "wed", "wee", "wet", "who", "why", "wig", "win", "wit", "wiz", "woe", "wok", "woo", "wow", "wry", "yak", "yes", "yet",
        "yob", "you", "yuk", "yum", "yup", "zag", "zap", "zed", "zen", "zig", "zip", "zit", "zoo"})

		Randomize()
		Return ThreeLetterWords(WordRand.Next(0, ThreeLetterWords.Count)).ToUpper
	End Function
    Function Get4Letter() As String
		Dim FourLetterWords As New List(Of String)({"abba", "abet", "able", "ably", "aced", "aces", "achy", "acid", "acne", "acre", "acts", "adds", "aeon", "aero", "afar",
					"agar", "aged", "ages", "agog", "ahem", "ahoy", "aide", "aids", "ails", "aims", "ajar", "akin", "ally", "aloe", "alow",
					"alps", "also", "alto", "amen", "amia", "amid", "ammo", "amok", "amps", "anon", "ante", "anti", "apes", "apex", "aqua",
					"arch", "arcs", "area", "arid", "arms", "army", "arts", "arty", "atom", "atop", "aunt", "aura", "auto", "avid", "avow",
					"away", "awed", "axed", "axel", "axes", "axis", "axle", "babe", "baby", "back", "bags", "bail", "bait", "bake", "bald",
					"ball", "balm", "bals", "band", "bane", "bang", "bank", "bare", "barf", "bark", "barm", "barn", "bars", "base", "bash",
					"bask", "bass", "bath", "bats", "batt", "baud", "bays", "bead", "beak", "beam", "bear", "beat", "beau", "beds", "beef",
					"been", "beep", "beer", "bees", "beet", "begs", "bell", "belt", "bend", "bent", "berm", "best", "bets", "bevy", "beys",
					"bias", "bibb", "bigs", "bike", "bile", "bill", "bind", "bins", "bird", "blam", "bled", "bloc", "blub", "blue", "blur",
					"boar", "boat", "body", "bold", "bole", "boll", "bolo", "bolt", "bomb", "bond", "bone", "bong", "boom", "boys", "brag",
					"brat", "braw", "bred", "bulb", "bulk", "bull", "bump", "bung", "bunk", "burp", "bush", "bust", "buzz", "byte", "cabs",
					"cafe", "cage", "cagy", "cake", "calf", "call", "calm", "cane", "cans", "cape", "caps", "carb", "card", "care", "carp",
					"cars", "cart", "cash", "cask", "cast", "cats", "cave", "cent", "chat", "chaw", "chef", "chin", "chip", "chop", "chow",
					"chug", "chum", "ciao", "cite", "city", "clad", "clag", "clam", "clan", "clap", "claw", "clay", "clef", "clip", "club",
					"clue", "coal", "coat", "coax", "coco", "coda", "code", "cogs", "coil", "coin", "cola", "cold", "cope", "cops", "copy",
					"cord", "core", "cork", "corn", "cost", "cosy", "cozy", "crab", "crag", "cram", "craw", "croc", "crop", "crow", "crud",
					"crux", "cube", "cubs", "cued", "cues", "cuff", "cull", "cult", "cups", "curb", "curd", "cure", "curl", "curt", "cusp",
					"cute", "cuts", "cyan", "cyst", "czar", "dabs", "daft", "dame", "damp", "dams", "dank", "dark", "dart", "dash", "data",
					"date", "dawn", "days", "daze", "dead", "deaf", "deal", "dear", "debt", "deck", "deed", "deem", "deep", "deer", "deft",
					"defy", "deli", "dell", "demo", "dent", "desk", "deva", "dews", "dial", "dice", "died", "diet", "digs", "dill", "dime",
					"dine", "ding", "dips", "dire", "dirt", "disc", "dish", "disk", "diva", "doat", "dock", "does", "dogs", "dogy", "dojo",
					"dole", "doll", "dols", "dome", "done", "doom", "door", "dope", "dopy", "dork", "dose", "dote", "dots", "dour", "dove",
					"down", "dows", "doze", "dozy", "drab", "drag", "dram", "drat", "draw", "drew", "drib", "drip", "drop", "drug", "drum",
					"dubs", "duce", "duck", "duct", "dude", "duds", "duel", "dues", "duet", "duff", "dull", "dumb", "dump", "dune", "dung",
					"dupe", "dusk", "dust", "duty", "dyed", "each", "earl", "earn", "ears", "ease", "east", "easy", "eats", "eaux", "ebbs",
					"echo", "edge", "edgy", "edit", "eels", "eggs", "eggy", "egos", "elks", "else", "emit", "emmy", "ends", "envy", "eons",
					"epic", "eras", "ergo", "eros", "euro", "ever", "eves", "evil", "exam", "exit", "eyed", "face", "fact", "fade", "fads",
					"fail", "fair", "fake", "fall", "fame", "fang", "farm", "fart", "fast", "faux", "fawn", "faze", "fear", "feat", "feed",
					"feel", "fees", "feet", "fell", "felt", "fend", "fern", "fess", "fest", "feta", "fete", "feud", "fibs", "fill", "film",
					"filo", "fils", "find", "fine", "firm", "firn", "fish", "fist", "fits", "five", "fizz", "flab", "flag", "flak", "flam",
					"flan", "flap", "flat", "flaw", "flax", "flea", "fled", "flee", "flew", "flex", "fley", "flic", "flip", "flog", "flop",
					"flow", "flub", "flue", "flux", "foal", "foam", "foil", "fobs", "fold", "folk", "font", "food", "fool", "foot", "ford",
					"form", "fort", "foul", "four", "fowl", "free", "fret", "frog", "from", "frow", "fuel", "full", "fume", "funk", "furl",
					"fury", "fuse", "fuss", "gaff", "gags", "gain", "gait", "gala", "gama", "game", "gang", "gaps", "gasp", "gate", "gave",
					"gawk", "gaze", "gear", "geek", "gems", "gene", "gift", "gigs", "gild", "gill", "gilt", "gimp", "girl", "gist", "give",
					"glad", "glam", "glow", "glue", "glug", "glum", "glut", "gnat", "gnaw", "goad", "goal", "goat", "gobo", "goby", "gods",
					"goer", "goes", "gogo", "gold", "golf", "gone", "gong", "good", "goof", "goon", "gore", "gory", "gosh", "goth", "gout",
					"gown", "grab", "grad", "gram", "gran", "grey", "grin", "grip", "grit", "grow", "guff", "guid", "gulf", "gull", "gulp",
					"guls", "gums", "gunk", "guns", "guru", "gush", "gust", "guts", "guys", "gyro", "hack", "hail", "hair", "half", "hall",
					"halt", "hand", "hang", "hard", "hare", "hark", "harm", "harp", "hash", "hats", "have", "hawk", "haws", "haze", "head",
					"heal", "heap", "hear", "heat", "heck", "heed", "heel", "heft", "heil", "heir", "held", "helm", "helo", "help", "heme",
					"hemp", "hems", "hens", "hent", "herb", "herd", "here", "herl", "herm", "hero", "hick", "hide", "high", "hill", "hilt",
					"hips", "hire", "hiss", "hist", "hits", "hive", "hoar", "hoax", "hobo", "hobs", "hock", "hogs", "hold", "holy", "home",
					"hone", "hood", "hook", "hoop", "hoot", "hope", "hops", "horn", "hose", "host", "hots", "hour", "hove", "hues", "huff",
					"huge", "hugs", "hump", "hums", "hung", "hunt", "hurl", "hurt", "hush", "husk", "huts", "hymn", "hype", "ibex", "iced",
					"ices", "icon", "idea", "idle", "idly", "idol", "idyl", "iffy", "inch", "info", "inks", "inro", "into", "ions", "iota",
					"ired", "iron", "item", "jail", "jake", "jams", "jaws", "jazz", "jean", "jerk", "jess", "jest", "jilt", "joey", "jogs",
					"join", "jolt", "josh", "joss", "jota", "jots", "jouk", "jowl", "jows", "joys", "juba", "jube", "juco", "judo", "juga",
					"jugs", "jump", "junk", "jury", "just", "kale", "keel", "keen", "keep", "kemp", "keno", "keys", "kick", "kids", "kill",
					"kiln", "kilo", "kilt", "kind", "king", "kink", "kirk", "kiss", "kite", "knap", "knee", "knew", "knit", "knob", "knop",
					"know", "labs", "lace", "lack", "lacy", "lads", "lady", "laid", "lain", "lair", "lake", "lamb", "lame", "lamp", "land",
					"lane", "laps", "lard", "lark", "lash", "last", "late", "lava", "lawn", "laws", "lays", "lazy", "lead", "leaf", "leak",
					"lean", "leap", "leek", "left", "legs", "lens", "lent", "lept", "leud", "levy", "lewd", "leys", "liar", "lice", "lick",
					"lied", "lies", "lieu", "life", "lift", "like", "lilo", "lily", "limb", "lime", "link", "lips", "lira", "lisp", "list",
					"live", "load", "loaf", "loam", "loan", "lobe", "lock", "logo", "logs", "lone", "long", "look", "loop", "loos", "loot",
					"lope", "lops", "lord", "lore", "lorn", "lory", "lose", "loss", "lost", "loth", "lots", "loud", "loup", "lour", "lout",
					"love", "lowe", "lown", "lows", "luck", "lump", "lung", "lush", "lust", "mace", "mach", "mack", "made", "mage", "mail",
					"maim", "main", "make", "mako", "male", "mall", "many", "maps", "mash", "mask", "mats", "maul", "mayo", "mays", "maze",
					"meal", "mean", "meat", "meds", "meet", "mega", "megs", "meld", "mell", "melt", "meme", "memo", "mend", "menu", "meow",
					"mesh", "mess", "meta", "mews", "meze", "mhos", "mice", "mics", "mild", "mind", "mine", "mini", "mink", "mint", "minx",
					"miss", "mist", "mite", "moan", "moat", "mobs", "mock", "mode", "mods", "mono", "mood", "moon", "more", "morn", "moss",
					"most", "move", "much", "muck", "mugs", "mule", "murk", "muse", "mush", "musk", "must", "mute", "myth", "nada", "naff",
					"nags", "nail", "naps", "near", "neck", "neon", "nerd", "nets", "news", "newt", "next", "nibs", "nice", "nick", "nide",
					"nobs", "nock", "node", "norm", "nose", "nosh", "nosy", "noun", "nous", "nova", "nubs", "nude", "nuke", "null", "numb",
					"nuns", "nurd", "nuts", "oars", "oast", "oath", "oats", "obey", "oboe", "odds", "ogle", "ogre", "ohed", "ohms", "oils",
					"oily", "oink", "okas", "omen", "okay", "omit", "once", "ones", "only", "ooze", "oozy", "open", "opes", "oral", "ours",
					"oust", "outs", "oven", "over", "ovum", "owed", "owes", "owls", "owns", "pace", "pack", "pads", "page", "paid", "pail",
					"pain", "pair", "pale", "pall", "palm", "paly", "pane", "parr", "pars", "part", "pass", "past", "pate", "path", "pats",
					"paty", "pave", "pawl", "pawn", "paws", "pays", "peak", "peal", "peas", "peat", "pecs", "peek", "peel", "peen", "peep",
					"peer", "pees", "pegs", "pelt", "pens", "perk", "perm", "pert", "pets", "pews", "pias", "pica", "pied", "pier", "pies",
					"pill", "pine", "ping", "pink", "pins", "pint", "pipe", "pips", "pita", "pith", "pits", "pity", "plan", "play", "plea",
					"pleb", "pled", "plew", "plex", "plie", "plod", "plop", "plot", "plow", "ploy", "plug", "plum", "plus", "pods", "poem",
					"poet", "pogy", "poke", "poky", "pole", "polo", "poly", "pond", "pong", "pony", "poop", "pope", "pork", "pose", "posh",
					"post", "pots", "pour", "pout", "pram", "prat", "prey", "prim", "prod", "prom", "prop", "pros", "prow", "puck", "punk",
					"puns", "punt", "puny", "quad", "quag", "quid", "quip", "quit", "quiz", "race", "rack", "racy", "raft", "raid", "rail",
					"rain", "rais", "rake", "ramp", "rams", "rank", "rant", "rape", "raps", "rare", "rase", "rash", "rasp", "rath", "rats",
					"rave", "raws", "raya", "rays", "raze", "razz", "read", "real", "ream", "reap", "rear", "rebs", "reck", "recs", "redd",
					"rede", "redo", "reds", "reed", "reef", "reek", "reel", "rees", "rent", "ribs", "rice", "rich", "ride", "rift", "rigs",
					"rile", "riot", "ripe", "rips", "rise", "risk", "ritz", "roam", "roar", "robe", "robs", "rock", "rocs", "rode", "rods",
					"roof", "rook", "room", "rope", "ropy", "rose", "rosy", "rote", "rows", "rube", "rude", "rued", "rugs", "ruin", "rule",
					"ruly", "rump", "rums", "rune", "rung", "runs", "runt", "ruse", "rush", "rusk", "rust", "ruth", "ruts", "sack", "sacs",
					"safe", "saga", "sage", "sagy", "said", "sail", "sake", "sall", "salt", "same", "sand", "sane", "sash", "sass", "save",
					"saws", "says", "scab", "scam", "scan", "scar", "scat", "scop", "scot", "scow", "scum", "seal", "seam", "sear", "seas",
					"seat", "seed", "seek", "seel", "seem", "seen", "seep", "seer", "sees", "self", "sell", "semi", "send", "sene", "sent",
					"sept", "shag", "shah", "sham", "shed", "shes", "shin", "ship", "shiv", "shop", "shot", "shut", "sick", "sics", "side",
					"sign", "silk", "sill", "silo", "silt", "sine", "sing", "sink", "sins", "sipe", "sips", "sire", "sirs", "site", "sith",
					"sits", "size", "skew", "skid", "skim", "skin", "skip", "slab", "slag", "slam", "slap", "slat", "slay", "sled", "slew",
					"slid", "slim", "slip", "slit", "slob", "sloe", "slog", "slot", "slow", "slub", "slug", "slum", "smog", "smug", "smut",
					"snag", "snap", "snob", "snog", "snot", "snow", "snub", "snug", "soak", "soap", "soar", "soba", "sobs", "soca", "sock",
					"soda", "sods", "sofa", "soft", "soil", "sold", "sole", "some", "soms", "sone", "song", "sons", "sook", "soon", "soot",
					"sort", "soth", "soul", "soup", "sour", "sous", "sown", "sows", "soya", "soys", "spae", "spam", "span", "spar", "spat",
					"spay", "sped", "spew", "spin", "spit", "spot", "stab", "stag", "star", "stat", "stem", "step", "stir", "stub", "stud",
					"stun", "such", "suds", "sued", "suit", "suks", "sulk", "sulu", "sumo", "sump", "sums", "sung", "sunk", "surd", "sure",
					"surf", "suss", "swab", "swag", "swam", "swan", "swap", "swat", "sway", "swig", "swim", "swob", "swop", "swot", "swum",
					"tabs", "tack", "taco", "tact", "tads", "tags", "tail", "tain", "taka", "take", "tale", "tali", "talk", "tall", "tame",
					"tamp", "tank", "tans", "taos", "tapa", "tape", "taps", "tare", "tarn", "taro", "tarp", "tars", "tart", "task", "tass",
					"tate", "taxi", "teal", "team", "tear", "tech", "teel", "teem", "teen", "tell", "tels", "temp", "tend", "tens", "tent",
					"test", "text", "thae", "that", "thaw", "thee", "them", "then", "they", "thin", "this", "thru", "thud", "thug", "thus",
					"tick", "tics", "tide", "tidy", "tied", "tier", "ties", "tiff", "tike", "tiki", "tile", "till", "tils", "tilt", "time",
					"tine", "ting", "tins", "tint", "tiny", "tipi", "tips", "tire", "tirl", "tiro", "titi", "tivy", "toad", "toby", "tods",
					"tody", "toea", "toed", "toes", "toff", "toft", "tofu", "toga", "togs", "toil", "toit", "toke", "tola", "told", "tole",
					"toll", "tomb", "tome", "toms", "tone", "tong", "tons", "tony", "took", "tool", "toom", "toon", "toot", "tope", "toph",
					"tops", "tora", "tory", "tosh", "toss", "tote", "tour", "tout", "town", "toys", "trad", "tram", "trap", "tray", "tree",
					"tref", "trek", "tres", "tret", "trey", "trig", "trim", "trio", "trip", "trod", "trog", "trop", "trot", "troy", "true",
					"trug", "tsar", "tsks", "tuba", "tube", "tubs", "tuck", "tugs", "tuna", "tune", "tung", "turf", "turk", "turn", "tush",
					"tusk", "tuts", "twas", "twee", "twig", "twin", "twit", "twos", "tyke", "tyne", "type", "typo", "tyre", "tzar", "ugly",
					"ulva", "undo", "unit", "unto", "upon", "urea", "urge", "uric", "urns", "used", "user", "uses", "vail", "vain", "vair",
					"vale", "vamp", "vane", "vary", "vase", "vast", "vats", "veal", "veer", "veil", "vein", "veld", "vend", "vent", "vera",
					"verb", "vert", "very", "vest", "veto", "vets", "vext", "vial", "vibe", "vice", "vide", "vids", "vied", "vier", "vies",
					"view", "vile", "vine", "vino", "viny", "visa", "vise", "vita", "viva", "vive", "voes", "void", "vole", "volt", "vote",
					"vows", "wack", "waft", "wage", "wags", "waif", "wail", "wain", "wait", "wake", "wale", "walk", "wall", "wand", "wane",
					"want", "ward", "ware", "warm", "warn", "warp", "wars", "wart", "wary", "wash", "wasp", "watt", "wave", "wavy", "wawl",
					"waws", "waxy", "wear", "webs", "weds", "weed", "week", "weel", "ween", "weep", "weer", "wees", "weld", "well", "welt",
					"wend", "wens", "wept", "were", "what", "when", "whew", "whey", "whim", "whin", "whip", "whir", "whit", "whiz", "whoa",
					"wich", "wick", "wide", "wife", "wigs", "wild", "wile", "will", "wilt", "wily", "wimp", "wind", "wine", "wing", "wink",
					"wino", "wins", "winy", "wipe", "wire", "wiry", "wise", "wish", "wisp", "wiss", "wist", "wite", "with", "wits", "wive",
					"woke", "wolf", "womb", "wood", "woof", "wool", "woos", "wore", "work", "worm", "worn", "wort", "wost", "wots", "wove",
					"wows", "wrap", "wren", "xyst", "yack", "yald", "yams", "yang", "yank", "yard", "yare", "yawn", "yeah", "yean", "year",
					"yell", "yelp", "yoga", "yoke", "your", "yous", "yuca", "yuch", "yuck", "yule", "zags", "zany", "zero", "zest", "zeta",
					"zigs", "zinc", "zips", "zits", "zone", "zonk", "zoom", "zoon", "zoos"})

		Randomize()
		Return FourLetterWords(WordRand.Next(0, FourLetterWords.Count)).ToUpper

	End Function
	Private Sub GenerateLetterCode()
		' will read a given text file of words and produce VB code to compile a list of words
		Dim reader As New IO.StreamReader("D:\ICT & CS Dropbox\Lee Minett\Coding\VB & VBA Code\Skill Challenge\My Project\4 letter words.txt")
		Dim writer As New IO.StreamWriter("D:\ICT & CS Dropbox\Lee Minett\Coding\VB & VBA Code\Skill Challenge\My Project\4 Letter code.txt")

		Dim done As Boolean = False
		Dim word As String
		Dim linecount As Short = 0
		Dim row As New Text.StringBuilder
		Dim ThreeLetterWords As New List(Of String)({"22", "44"})

		row.Append("Dim FourLetterWords as new list(of string)({")
		Do
			Do Until reader.EndOfStream Or linecount = 15
				word = reader.ReadLine
				row.Append(ChrW(34) & word & ChrW(34))
				If linecount < 15 And Not reader.EndOfStream Then
					row.Append(", ")
					linecount += 1
				End If
			Loop
			linecount = 0
			If Not reader.EndOfStream Then
				row.Append(" _")
			Else
				row.Append("})")
				done = True
			End If
			writer.WriteLine(row.ToString)
			row.Clear()
			row.Append(ChrW(9) & ChrW(9) & ChrW(9))
		Loop Until done

		reader.Close()
		writer.Close()
	End Sub
End Module
