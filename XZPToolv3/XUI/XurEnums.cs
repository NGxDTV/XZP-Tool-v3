using System;

namespace XZPToolv3.XUI
{
    public enum XUI_NAMEDFRAME_COMMAND
    {
        XUI_CMD_PLAY,
        XUI_CMD_STOP,
        XUI_CMD_GOTO,
        XUI_CMD_GOTOANDPLAY,
        XUI_CMD_GOTOANDSTOP,
        XUI_CMD_COUNT
    }

    public enum XUI_INTERPOLATE
    {
        XUI_INTERPOLATE_LINEAR,
        XUI_INTERPOLATE_NONE,
        XUI_INTERPOLATE_EASE
    }

    public enum XUR8_SECTION_NAME
    {
        SECTION_KEYD,
        SECTION_COLR,
        SECTION_CUST,
        SECTION_DATA,
        SECTION_DBUG,
        SECTION_FLOT,
        SECTION_KEYP,
        SECTION_NAME,
        SECTION_QUAT,
        SECTION_STRN,
        SECTION_VECT,
        SECTION_COUNT,
    }

    public enum XUIELEM_PROP_TYPE
    {
        XUI_EPT_EMPTY,
        XUI_EPT_BOOL,
        XUI_EPT_INTEGER,
        XUI_EPT_UNSIGNED,
        XUI_EPT_FLOAT,
        XUI_EPT_STRING,
        XUI_EPT_COLOR,
        XUI_EPT_VECTOR,
        XUI_EPT_QUATERNION,
        XUI_EPT_OBJECT,
        XUI_EPT_CUSTOM
    }
}
