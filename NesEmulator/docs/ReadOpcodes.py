import json


with open("opcodes.txt", "r") as f:
    lines = [line.strip().split("\t") for line in f.readlines()]

opcodes = {}

for hi in range(16):
    for lo in range(16):
        code = lines[1 + hi][1 + lo]
        if code != "---":
            opcodes[hex(16 * hi + lo)] = code

with open("opcodes.json", "w+") as f:
    json.dump(opcodes, f, indent=4)
