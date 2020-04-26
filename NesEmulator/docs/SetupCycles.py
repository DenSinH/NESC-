from pprint import pprint
import json


with open("detailed.txt", "r") as f:
    lines = [line[:-1] for line in f.readlines() if line.strip()]

address_modes = {
    "accumulator": "A",
    "absolute": "abs",
    "absolute,X": "abs,X",
    "absolute,Y": "abs,Y",
    "immidiate": "#",
    "implied": "impl",
    "indirect": "ind",
    "(indirect,X)": "X,ind",
    "(indirect),Y": "ind,Y",
    "relative": "rel",
    "zeropage": "zpg",
    "zeropage,X": "zpg,X",
    "zeropage,Y": "zpg,Y"
}

i = 0
while i < len(lines):
    instruction = lines[i].split(" ")[0]
    print(instruction)
    i += 1

    opcycles = {"internal": 0}

    collect = False
    while i < len(lines) and lines[i][0] == " ":
        if collect:
            data = lines[i].strip(" ").split(" ")
            mode = address_modes[data[0]]
            cycles = data[-1]

            opcycles[mode] = int(cycles[0])

            print(f"        elif mode == \"{mode}\":\n            return {cycles}".replace("*", "  # *"))

        elif "--------------------------------------------" in lines[i]:
            collect = True
        i += 1

    with open(f"../data/cycles/{instruction}.json", "w+") as f:
        json.dump(opcycles, f, indent=4)

    print("        else:\n            raise Exception(f\"mode '{mode}' invalid for " + instruction + "\")")
