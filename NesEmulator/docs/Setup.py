from pprint import pprint


with open("detailed.txt", "r") as f:
    lines = [line[:-1] for line in f.readlines() if line.strip()]

i = 0
with open("CPUsign.tmp", "w+") as f:
    while i < len(lines):
        info = lines[i]
        i += 1

        while i < len(lines) and lines[i][0] == " ":
            info += "\n" + lines[i]
            i += 1

        if "implied" in info:
            arguments = ""
            assertion = ""
        else:
            arguments = ", oper: 'Byte'"
            assertion = "assert isinstance(oper, Byte)\n"

        info = info.replace("\n    ", "\n            ")

        f.write(
            f'''
    def {info[:3]}(self{arguments}, mode):
        """
        {info}
        """
        {assertion}
        pass
'''
        )

        print(info)
        print("***")
