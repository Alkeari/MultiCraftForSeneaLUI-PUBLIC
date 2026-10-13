# MultiCraft for SeneaL UI

MultiCraft for SeneaL UI lets MultiCraft and SeneaL UI's crafting window work together, so a chosen amount crafts as one batch with one timer on every SeneaL UI version.

> **AS OF SeneaL UI v1.1.8+ THIS IS NO LONGER NEEDED.**

---

## Availability

- [Nexus Mods](https://www.nexusmods.com/valheim/mods/4151)
- [Thunderstore](https://thunderstore.io/c/valheim/p/Alkeari/MultiCraft_for_SeneaL_UI/)

## What It Does

- **Not needed from SeneaL UI 1.1.8** - SeneaL UI 1.1.8 and later craft a chosen amount with their own stepper, so you can remove both MultiCraft and this mod.
- **Keeps MultiCraft from breaking it** - if you keep MultiCraft with SeneaL UI 1.1.8 or later, keep this mod too: without it, MultiCraft resets the amount and a batch crafts one item.
- **D-Pad steps the amount** - with MultiCraft's gamepad input on, D-Pad Left and Right change SeneaL UI's craft amount, and the left stick still changes category.
- **An amount control on SeneaL UI 1.1.7** - a "- N +" control beside the craft button sets MultiCraft's amount: Shift+click by its bulk step, Ctrl+click to the most you can craft or back to 1.
- **One craft, one timer** - on SeneaL UI 1.1.7 a click crafts the whole batch at once, and the button stays grayed out until you carry the materials for all of it.

---

## Requirements

- Verified on Valheim 1.0.17.

## Installation

1. Open a crafting station in game, pick a recipe and set an amount: crafting makes that many items in one craft.

To update, install the new version over the old one.

## Configuration

It has no settings of its own. MultiCraft's `EnableGamepadInput` turns the D-Pad control on or off, and on SeneaL UI 1.1.7 MultiCraft's `CapMaximumCrafts`, `GreyOutPlusAtMax` and `GamepadBulkStep` also shape the amount control.

## Compatibility

- Client-side: only you need it, and a server does not.
- Removing it changes nothing in a world or a character.
- With SeneaL UI's crafting window turned off, MultiCraft's own buttons on the vanilla crafting panel work as they do without this mod.

## Support

- Include your game version, the mod version, and the other mods you have installed.

## License

License terms are in the [Alkeari License Agreement](https://gist.github.com/Alkeari/2c6ec0cdf3dafee375b1a00b28ca190a).
