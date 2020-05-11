# NESC-
Changed to C# for NES emulator

It is now basically functional. There are a few issues still:

- APU Triangle wave doesn't always stop when it should (causing there to be a continuous low tone in the underground levels of Super Mario Bros for example)
- Mapper 1 doesn't work properly (causing Zelda to still load, but garbage sprites show up whenever there is a horizontal screen transition)
- Metroid doesn't load at all
- Mapper 7 seems to work, but Battletoads requires too precise timing (which my emulator does not have)
- The debug screen causes the game to tear a bit more and become a bit slower (because of the constant updates probably, I tried to keep it as minimal as possible)
- There is no possibility to make save files/savestates. This is not an actual feature of an NES, but is very useful.

I had a lot of fun making this!