import json, re, os

ROOT = r"W:\PMCR_O\PMCR-O-Marketplace"

CSUITE = ["ceo","cfo","cto","coo","cmo","cro","clo","chro","chief-of-staff"]
ENGINE = ["orchestrator","planner","maker","checker","reflector"]

CATEGORY = {
    "ceo":"executive","cfo":"executive","cto":"executive","coo":"executive",
    "cmo":"executive","cro":"executive","clo":"executive","chro":"executive",
    "chief-of-staff":"staff",
    "orchestrator":"engine","planner":"engine","maker":"engine","checker":"engine","reflector":"engine",
}
DISPLAY = {
    "ceo":"Chief Executive Officer","cfo":"Chief Financial Officer","cto":"Chief Technology Officer",
    "coo":"Chief Operations Officer","cmo":"Chief Marketing Officer","cro":"Chief Revenue Officer",
    "clo":"Chief Legal Officer","chro":"Chief People Officer","chief-of-staff":"Chief of Staff",
    "orchestrator":"Orchestrator (PMCR-O Loop)","planner":"Planner (PMCR-O Loop)","maker":"Maker (PMCR-O Loop)",
    "checker":"Checker (PMCR-O Loop)","reflector":"Reflector (PMCR-O Loop)",
}

def plugin_dir(name):
    if name in CSUITE:
        return os.path.join(ROOT, "plugins", "pmcro-csuite", "skills", name)
    else:
        return os.path.join(ROOT, "plugins", "pmcro-engine", "skills", name)

def fix_plugin_json(name):
    pdir = plugin_dir(name)
    pjson_path = os.path.join(pdir, ".claude-plugin", "plugin.json")
    with open(pjson_path, "r", encoding="utf-8-sig") as f:
        data = json.load(f)
    data["displayName"] = DISPLAY[name]
    data["author"] = {"name": "Shawn Delaine Bellazan", "email": "shawn2024bellazan@gmail.com"}
    data["homepage"] = "https://github.com/ShawnDelaineBellazanLoop/PMCR-O-Marketplace"
    data["repository"] = "https://github.com/ShawnDelaineBellazanLoop/PMCR-O-Marketplace"
    data["license"] = "UNLICENSED"
    data["category"] = CATEGORY[name]
    kw = ["pmcro", "colony", name]
    if name in CSUITE:
        kw += ["c-suite", "executive-domain"]
    else:
        kw += ["pmcro-loop", "engine"]
    data["keywords"] = kw
    if data.get("description","").strip().lower() == f"{name} skill":
        data["description"] = f"{DISPLAY[name]} domain skill in the PMCR-O Colony."
    ordered = {}
    for k in ["name","displayName","description","version","author","homepage","repository","license","keywords","category"]:
        if k in data:
            ordered[k] = data[k]
    with open(pjson_path, "w", encoding="utf-8", newline="\n") as f:
        json.dump(ordered, f, indent=2, ensure_ascii=False)
        f.write("\n")
    return True

results = []
for name in CSUITE + ENGINE:
    try:
        fix_plugin_json(name)
        results.append((name, "plugin.json OK"))
    except Exception as e:
        results.append((name, "plugin.json ERROR: " + str(e)))

for r in results:
    print(r)
