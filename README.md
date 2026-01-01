# XZPToolv3

XZPToolv3 is a Windows GUI tool for working with Xbox 360 XZP archives, plus XUI/XUR viewing and conversion.

## Build

- Target framework: .NET Framework 4.7.2
- Open the solution and build in Visual Studio

## Feature Matrix

Legend: Yes, No, Partial

| Feature                         | XZP Tool v2  | XuiWorkshop | XZPToolv3    |
| ------------------------------- | ------------ | ----------- | ------------ |
| Open/browse XZP archive         | ✅           | ❌          | ✅           |
| Extract all files               | ✅           | ❌          | ✅           |
| Extract selected entries        | ✅           | ❌          | ✅           |
| Add files/folders to archive    | ✅           | ❌          | ✅           |
| Rename entries inside archive   | ✅ (rebuild) | ❌          | ✅ (rebuild) |
| Delete entries inside archive   | ❌           | ❌          | ✅ (rebuild) |
| Build XZP from folder           | ✅           | ❌          | ✅           |
| Convert XZP version (v1/v3)     | ✅           | ❌          | ✅           |
| Copy XZP archive                | ✅           | ❌          | ✅           |
| Drag-and-drop add               | ❌           | ❌          | ✅           |
| Drag out to extract             | ❌           | ❌          | ✅           |
| Search/filter entries           | ❌           | ❌          | ✅           |
| Sorting by column               | ❌           | ❌          | ✅           |
| XUI viewer (tree/properties)    | ❌           | ✅          | ✅           |
| XUR viewer (tree/properties)    | ❌           | ✅          | ✅           |
| XUI -> XUR conversion           | ❌           | ✅          | ✅           |
| XUR -> XUI conversion           | ❌           | ✅          | ✅           |
| Batch XUI/XUR conversion        | ❌           | ❌          | 🪧           |
| Timeline editor                 | ❌           | ✅          | ❌           |
| Timeline viewer                 | ❌           | ✅          | ✅           |
| XML extension list management   | ❌           | ✅          | ✅           |
| XAM/XUI class toggle            | ❌           | ✅          | ❌           |
| XUI Tool Version toggle         | ❌           | ✅          | ❌           |
| Add animations toggle           | ❌           | ✅          | ❌           |
| File association for .xur       | ❌           | ✅          | ❌           |
| Encrypted byte output templates | ❌           | ❌          | ✅           |

## Notes and Gaps

- **XZP Tool v2** does not implement direct delete inside archives. It rebuilds for rename and add operations but has no delete path in the archive logic.
- **XZP Tool v3** also rebuilds archives for add/rename/delete; it is not a block-level in-place editor.
- \*\*XuiWorkshop focuses on XUI/XUR editing and conversion, not XZP archives.
- **XZP Tool v3** does not include XuiWorkshop's full editor UI (property editing, timeline editing, and batch conversion).
- **XZPToolv3** includes encrypted byte output templates and RC4 helpers, which are not in v2 or XuiWorkshop.

## Credits

- NGxD TV
- XZP Tool v2
- XuiWorkshop sources
