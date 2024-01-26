using LibUsbDotNet.Info;
using LibUsbDotNet.Main;
using RetroSpy.Readers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;

namespace RetroSpy
{
    public class InputSource
    {
        public static readonly InputSource MISTER = new("mister", "MiSTer", false, false, true, false, true,3, (hostname, username, password, commandSub) => new SSHControllerReader(hostname, "/media/fat/retrospy/retrospy /dev/input/js{0}", MiSTerReader.ReadFromPacket, username, password, commandSub, 5000, true));
        //public static readonly InputSource MISTER = new InputSource("mister", "MiSTer", false, false, true, false, true,3, (hostname, username, password, commandSub) => new SSHControllerReader(hostname, "killall retrospy ; /media/fat/retrospy/retrospy /dev/input/js{0}", MiSTerReader.ReadFromPacket, username, password, commandSub, 5000));

        public static readonly InputSource POCKET = new("pocket", "Analogue Pocket", false, false, true, false, false, 0, (hostname, username, password) => new SSHControllerReader(hostname, "sudo pkill -9 usb-mitm ; sudo usb-mitm 2> /dev/null -b -dddddddd", Xbox360Reader.ReadFromPacket, username, password, null, 0));

        public static readonly InputSource CLASSIC = new("classic", "Atari/Commodore/SMS", true, false, false, false, false, 3, (port, useLagFix) => new SerialControllerReader(port, useLagFix, Classic.ReadFromPacket));
        public static readonly InputSource DRIVINGCONTROLLER = new("drivingcontroller", "Atari Driving Controller", true, false, false, false, false, 3, (port, useLagFix) => new SerialControllerReader(port, useLagFix, DrivingController.ReadFromPacket));
        public static readonly InputSource ATARIKEYBOARD = new("atarikeyboard", "Atari Keyboard Controller", true, false, false, false, false, 3, (port, useLagFix) => new SerialControllerReader(port, useLagFix, AtariKeyboard.ReadFromPacket));
        public static readonly InputSource PADDLES = new("paddles", "Atari Paddles", true, false, false, true, false, 3, (port, port2, useLagFix) => new SerialControllerReader2(port, port2, useLagFix, Paddles.ReadFromPacket, Paddles.ReadFromSecondPacket));

        public static readonly InputSource ATARI5200 = new("atari5200", "Atari 5200", true, false, false, true, false, 3, (port, port2, useLagFix) => new SerialControllerReader2(port, port2, useLagFix, SuperNESandNES.ReadFromPacketAtari52001, SuperNESandNES.ReadFromPacketAtari52002));
        public static readonly InputSource JAGUAR = new("jaguar", "Atari Jaguar", true, false, false, false, false, 3, (port, useLagFix) => new SerialControllerReader(port, useLagFix, SuperNESandNES.ReadFromPacketJaguar));
        public static readonly InputSource ATARIVCS = new("vcs", "Atari VCS", false, false, true, false, false, 0, (hostname, username, password) => new SSHControllerReader(hostname, "sudo pkill -9 usb-mitm ; sudo usb-mitm 2> /dev/null -V", VCS.ReadFromPacket, username, password, null, 0));

        public static readonly InputSource PIPPIN = new("pippin", "Bandai Pippin", true, false, false, false, false, 3, (port, useLagFix) => new SerialControllerReader(port, useLagFix, Pippin.ReadFromPacket));

        public static readonly InputSource EVERCADE = new("evercade", "Blaze Evercade Vs", false, false, true, false, false, 0, (hostname, username, password) => new SSHControllerReader(hostname, "sudo pkill -9 usb-mitm ; sudo usb-mitm 2> /dev/null -n", EVS.ReadFromPacket, username, password, null, 0));

        public static readonly InputSource COLECOVISION = new("colecovision", "ColecoVision", true, false, false, true, false, 3, (port, port2, useLagFix) => new SerialControllerReader2(port, port2, useLagFix, ColecoVision.ReadFromPacket, ColecoVision.ReadFromSecondColecoVisionController));

        public static readonly InputSource CDTV = new("cdtv", "Commodore CDTV", true, false, false, false, false, 3, (port, useLagFix) => new SerialControllerReader(port, useLagFix, Amiga.ReadFromPacket));
        public static readonly InputSource CD32 = new("cd32", "Commodore Amiga CD32", true, false, false, true, false, 3, (port, port2, useLagFix) => new SerialControllerReader2(port, port2, useLagFix, Amiga.ReadFromPacket, Amiga.ReadFromPacket2));
        public static readonly InputSource C64MINI = new("c64mini", "The C64 Mini", false, false, true, false, false, 0, (hostname, username, password) => new SSHControllerReader(hostname, "sudo pkill -9 usb-mitm ; sudo usb-mitm 2> /dev/null -z", C64mini.ReadFromPacket, username, password, null, 0));
        public static readonly InputSource A500MINI = new("a500", "The A500 Mini", false, false, true, false, false, 0, (hostname, username, password) => new SSHControllerReader(hostname, "sudo pkill -9 usb-mitm ; sudo usb-mitm 2> /dev/null -5", A500.ReadFromPacket, username, password, null, 0));

        public static readonly InputSource FMTOWNS = new("fmtowns", "Fujitsu FM Towns Marty", true, false, false, false, false, 3, (port, useLagFix) => new SerialControllerReader(port, useLagFix, SuperNESandNES.ReadFromPacketFMTowns));

        public static readonly InputSource INTELLIVISION = new("intellivision", "Mattel Intellivision", true, false, false, false, false, 3, (port, useLagFix) => new SerialControllerReader(port, useLagFix, SuperNESandNES.ReadFromPacketIntellivision));

#pragma warning disable CS8601 // Possible null reference assignment.
        public static readonly InputSource LINUX = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? new("linuxjoystick", "Linux Joystick", false, true, false, false, false, 3, (controllerId, useLagFix) => new LinuxJoystickReader(int.Parse(controllerId ?? "0", CultureInfo.CurrentCulture))) : null;
        public static readonly InputSource LINUXKEY = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? new("linuxkeyboard", "Linux Keyboard & Mouse (may need su/sudo)", false, false, false, false, false, 3, new LinuxMouseAndKeyboardReader()) : null;
#pragma warning restore CS8601 // Possible null reference assignment.

        public static readonly InputSource DOLPHIN = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? new("dolphin", "Dolphin + Mayflash GameCube Controller Adapter", false, true, false, false, false, 3, (controllerId, useLagFix) => new LibUsbControllerReader(controllerId ?? "1", 0x057E, 0x0337, ReadEndpointID.Ep01, 37, 9, 1, DolphinMayflashReader.ReadFromPacket))
                                                   : RuntimeInformation.IsOSPlatform(OSPlatform.Linux)   ? new("dolphin", "Dolphin + Mayflash GameCube Controller Adapter", false, true, false, false, false, 3, (controllerId, useLagFix) => new MMapControllerReader(controllerId ?? "1", "/dev/shm/gcadapter", 36, 9, 0, DolphinMayflashReader.ReadFromPacket))
                                                   :                                                       new("dolphin", "Dolphin + Mayflash GameCube Controller Adapter", false, true, false, false, false, 3, (controllerId, useLagFix) => new MMapControllerReader(controllerId ?? "1", "/tmp/gcadapter", 36, 9, 0, DolphinMayflashReader.ReadFromPacket));


        public static readonly InputSource XBOX = new("xbox", "Microsoft Xbox", false, false, true, false, false, 0, (hostname, username, password) => new SSHControllerReader(hostname, "sudo pkill -9 usb-mitm ; sudo usb-mitm 2> /dev/null -x", XboxReaderV2.ReadFromPacket, username, password, null, 0));
        public static readonly InputSource XBOX360 = new("xbox360", "Microsoft Xbox 360", false, false, true, false, false, 0, (hostname, username, password) => new SSHControllerReader(hostname, "sudo pkill -9 usb-mitm ; sudo usb-mitm 2> /dev/null -b -dddddddd", Xbox360Reader.ReadFromPacket, username, password, null, 0));

        public static readonly InputSource TG16 = new("tg16", "NEC TurboGrafx-16", true, false, false, false, false, 3, (port, useLagFix) => new SerialControllerReader(port, useLagFix, Tg16.ReadFromPacket));
        public static readonly InputSource PCFX = new("pcfx", "NEC PC-FX", true, false, false, false, false, 3, (port, useLagFix) => new SerialControllerReader(port, useLagFix, SuperNESandNES.ReadFromPacketPCFX));
        public static readonly InputSource TG16MINI = new("tg16mini", "NEC TurboGrafx-16 Mini", false, false, true, false, false,0, (hostname, username, password) => new SSHControllerReader(hostname, "sudo pkill -9 usb-mitm ; sudo usb-mitm 2> /dev/null -j", Tg16Mini.ReadFromPacket, username, password, null, 0));

        public static readonly InputSource NES = new("nes", "Nintendo NES", true, false, false, false, false, 3, (port, useLagFix) => new SerialControllerReader(port, useLagFix, SuperNESandNES.ReadFromPacketNES));
        public static readonly InputSource SNES = new("snes", "Nintendo SNES", true, false, false, false, false, 3, (port, useLagFix) => new SerialControllerReader(port, useLagFix, SuperNESandNES.ReadFromPacketSNES));
        public static readonly InputSource VIRTUALBOY = new("virtualboy", "Nintendo VirtualBoy", true, false, false, false, false, 3, (port, useLagFix) => new SerialControllerReader(port, useLagFix, SuperNESandNES.ReadFromPacketVB));
        public static readonly InputSource N64 = new("n64", "Nintendo 64", true, false, false, false, false, 3, (port, useLagFix) => new SerialControllerReader(port, useLagFix, Nintendo64.ReadFromPacket));
        public static readonly InputSource GAMECUBE = new("gamecube", "Nintendo GameCube", true, false, false, true, false, 3, (port, port2, useLagFix) => new SerialControllerReader2(port, port2, useLagFix, GameCube.ReadFromPacket, GameCube.ReadFromSecondPacket));
        public static readonly InputSource WII = new("wii", "Nintendo Wii", true, false, false, false, false, 3, (port, useLagFix) => new SerialControllerReader(port, useLagFix, WiiReaderV2.ReadFromPacket));
        public static readonly InputSource SWITCH = new("switch", "Nintendo Switch", false, false, true, false, false, 0, (hostname, username, password) => new SSHControllerReader(hostname, "sudo pkill -9 usb-mitm ; sudo usb-mitm 2> /dev/null -z", SwitchReader.ReadFromPacket, username, password, null, 0));

        public static readonly InputSource THREEDO = new("3do", "Panasonic 3DO", true, false, false, false, false, 3, (port, useLagFix) => new SerialControllerReader(port, useLagFix, ThreeDO.ReadFromPacket));

#pragma warning disable CS8601 // Possible null reference assignment.
        public static readonly InputSource PC360 = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? new ("pc360", "PC 360 Controller", false, true, false, false, false, 3, (controllerId, useLagFix) => new XInputReader(int.Parse(controllerId ?? "0", CultureInfo.CurrentCulture))) : null;
        public static readonly InputSource PAD = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? new("generic", "PC Generic Gamepad", false, true, false, false, false, 3, (controllerId, useLagFix) => new GamepadReader(int.Parse(controllerId ?? "0", CultureInfo.CurrentCulture))) : null;
        public static readonly InputSource PCKEYBOARD = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? new("pckeyboard", "PC Keyboard & Mouse", false, false, false, false, false, 3, new PCKeyboardReader()) : null;
#pragma warning restore CS8601 // Possible null reference assignment.

        public static readonly InputSource CDI = new("cdi", "Phillips CD-i", true, false, false, false, false, 3, (port, useLagFix) => new SerialControllerReader(port, useLagFix, CDi.ReadFromPacket));

        public static readonly InputSource SEGA = new("genesis", "Sega Genesis", true, false, false, false, false, 3, (port, useLagFix) => new SerialControllerReader(port, useLagFix, Sega.ReadFromPacket));
        public static readonly InputSource SATURN3D = new("saturn", "Sega Saturn", true, false, false, false, false, 3, (port, useLagFix) => new SerialControllerReader(port, useLagFix, SS3D.ReadFromPacket));
        public static readonly InputSource DREAMCAST = new("dreamcast", "Sega Dreamcast", true, false, false, false, false, 3, (port, useLagFix) => new SerialControllerReader(port, useLagFix, Dreamcast.ReadFromPacket));
        public static readonly InputSource GENMINI = new("genesismini", "Sega Genesis Mini 1", false, false, true, false, false,0, (hostname, username, password) => new SSHControllerReader(hostname, "sudo pkill -9 usb-mitm ; sudo usb-mitm 2> /dev/null -z", GenesisMiniReader.ReadFromPacket, username, password, null, 0));
        public static readonly InputSource GENMINI2 = new("genesismini2", "Sega Genesis Mini 2", false, false, true, false, false,0, (hostname, username, password) => new SSHControllerReader(hostname, "sudo pkill -9 usb-mitm ; sudo usb-mitm 2> /dev/null -6", GenesisMiniReader.ReadFromPacket, username, password, null, 0));

        public static readonly InputSource NEOGEO = new("neogeo", "SNK NeoGeo", true, false, false, false, false, 3, (port, useLagFix) => new SerialControllerReader(port, useLagFix, NeoGeo.ReadFromPacket));
        public static readonly InputSource NEOGEOMINI = new("neogeomini", "SNK NeoGeo Mini", false, false, true, false, false,0, (hostname, username, password) => new SSHControllerReader(hostname, "sudo pkill -9 usb-mitm ; sudo usb-mitm 2> /dev/null -g", NeoGeoMini.ReadFromPacket, username, password, null, 0));

        public static readonly InputSource PLAYSTATION2 = new("playstation", "Sony Playstation 1/2", true, false, false, false, false, 3, (port, useLagFix) => new SerialControllerReader(port, useLagFix, Playstation2.ReadFromPacket));
        public static readonly InputSource PS3 = new("playstation3", "Sony PlayStation 3", false, false, true, false, false, 0, (hostname, username, password) => new SSHControllerReader(hostname, "sudo pkill -9 usb-mitm ; sleep 1 ; sudo usb-mitm 2> /dev/null -u", PS3Reader.ReadFromPacket, username, password, null, 0));
        public static readonly InputSource PS4 = new("playstation4", "Sony PlayStation 4", false, false, true, false, false, 0, (hostname, username, password) => new SSHControllerReader(hostname, "sudo pkill -9 ds4drv ; sudo ds4drv --hidraw --dump-reports", PS4Reader.ReadFromPacket, username, password, null, 0));
        public static readonly InputSource PS4CRONUS = new("playstation4", "Sony PlayStation 4 via Cronus Zen", false, false, true, false, false, 0, (hostname, username, password) => new SSHControllerReader(hostname, "sudo pkill -9 usb-mitm ; sleep 1 ; sudo usb-mitm 2> /dev/null -q", PS4Reader.ReadFromPacket, username, password, null, 0));
        public static readonly InputSource PS4USB = new("playstation4", "Sony PlayStation 4 Wired Controllers", false, false, true, false, false, 0, (hostname, username, password) => new SSHControllerReader(hostname, "sudo pkill -9 usb-mitm ; sleep 1 ; sudo usb-mitm 2> /dev/null -7", PS4Reader.ReadFromPacket, username, password, null, 0));
        public static readonly InputSource PSCLASSIC = new("psclassic", "Sony PlayStation Classic", false, false, true, false, false, 0, (hostname, username, password) => new SSHControllerReader(hostname, "sudo pkill -9 usb-mitm ; sudo usb-mitm 2> /dev/null -y", SuperNESandNES.ReadFromPacketPSClassic_II, username, password, null, 0));

        public static readonly InputSource NUON = new("nuon", "VM Labs Nuon", true, false, false, false, false, 3, (port, useLagFix) => new SerialControllerReader(port, useLagFix, Nuon.ReadFromPacket));

        public static readonly InputSource VSMILE = new("vsmile", "VTech V.Smile", true, false, false, false, false, 3, (port, useLagFix) => new SerialControllerReader(port, useLagFix, VSmile.ReadFromPacket));
        public static readonly InputSource VFLASH = new("vflash", "VTech V.Flash", true, false, false, false, false, 3, (port, useLagFix) => new SerialControllerReader(port, useLagFix, VFlash.ReadFromPacket));

        public static readonly InputSource POCKET_II = new("pocket", "Analogue Pocket", true, false, false, false, false, 1, (port, useLagFix) => new SuperSerialControllerReader(port, useLagFix, true, XBox360Reader_II.ReadFromPacket));
        public static readonly InputSource XBOX360_II = new("xbox360", "Microsoft Xbox 360", true, false, false, false, false, 1, (port, useLagFix) => new SuperSerialControllerReader(port, useLagFix, true,  XBox360Reader_II.ReadFromPacket));
        public static readonly InputSource SWITCH_II = new("switch", "Nintendo Switch", true, false, false, false, false, 1, (port, useLagFix) => new SuperSerialControllerReader(port, useLagFix, true, SwitchReader_II.ReadFromPacket));
        public static readonly InputSource PS4_II = new("playstation4", "Sony PlayStation 4", true, false, false, false, false, 1, (port, useLagFix) => new SuperSerialControllerReader(port, useLagFix, true, PS4Reader_II.ReadFromPacket));
        public static readonly InputSource PS3_II = new("playstation3", "Sony PlayStation 3", true, false, false, false, false, 1, (port, useLagFix) => new SuperSerialControllerReader(port, useLagFix, true, PS3Reader_II.ReadFromPacket));
        public static readonly InputSource XBOX_II = new("xbox", "Microsoft Xbox", true, false, false, false, false, 1, (port, useLagFix) => new SuperSerialControllerReader(port, useLagFix, true, XboxReaderV2.ReadFromPacket));
        public static readonly InputSource GENMINI1_II = new("genesismini", "Sega Genesis Mini 1", true, false, false, false, false, 1, (port, useLagFix) => new SuperSerialControllerReader(port, useLagFix, true, GenesisMiniReader_II.ReadFromPacket));
        public static readonly InputSource GENMINI2_II = new("genesismini2", "Sega Genesis Mini 2", true, false, false, false, false, 1, (port, useLagFix) => new SuperSerialControllerReader(port, useLagFix, true, GenesisMiniReader_II.ReadFromPacket));
        public static readonly InputSource TG16_II = new("tg16mini", "NEC TurboGrafx-16 Mini", true, false, false, false, false, 1, (port, useLagFix) => new SuperSerialControllerReader(port, useLagFix, true, Tg16Mini_II.ReadFromPacket));
        public static readonly InputSource PSCLASSIC_II = new("psclassic", "Sony PlayStation Classic", true, false, false, false, false, 1, (port, useLagFix) => new SuperSerialControllerReader(port, useLagFix, true, SuperNESandNES.ReadFromPacketPSClassic_II));
        public static readonly InputSource A500MINI_II = new("a500", "The A500 Mini", true, false, false, false, false, 1, (port, useLagFix) => new SuperSerialControllerReader(port, useLagFix, false, A500.ReadFromPacket));
        public static readonly InputSource C64MINI_II = new("c64mini", "The C64 Mini", true, false, false, false, false, 1, (port, useLagFix) => new SuperSerialControllerReader(port, useLagFix, false, C64mini_II.ReadFromPacket));
        public static readonly InputSource EVERCADE_II = new("evercade", "Blaze Evercade Vs", true, false, false, false, false, 1, (port, useLagFix) => new SuperSerialControllerReader(port, useLagFix, true, EVS.ReadFromPacket));
        public static readonly InputSource NEOGEOMINI_II = new("neogeomini", "SNK NeoGeo Mini", true, false, false, false, false, 1, (port, useLagFix) => new SuperSerialControllerReader(port, useLagFix, true, NeoGeoMini_II.ReadFromPacket));
        public static readonly InputSource ATARIVCS_II = new("vcs", "Atari VCS", true, false, false, false, false, 1, (port, useLagFix) => new SuperSerialControllerReader(port, useLagFix, true, VCS.ReadFromPacket));
        public static readonly InputSource USBMOUSE = new("usbmouse", "Windows USB Mouse", true, false, false, false, false, 4, (port, useLagFix) => new SuperSerialControllerReader(port, useLagFix, false, USBMouse.ReadFromPacket));

        // Retired/Non-Functional
        //static public readonly InputSource MOUSETESTER = new InputSource("mousetester", "Mouse Tester", true, false, false, false, false, 3, (port, useLagFix) => new MouseTester3, (port));
        //static public readonly InputSource WII = new InputSource("wii", "Nintendo Wii", false, true, controllerId => new WiiReaderV1(int.Parse3, (controllerId)));
        //static public readonly InputSource TOUCHPADTESTER = new InputSource("touchpadtester", "TouchPad Tester", true, false, false, false, port => new TouchPadTester3, (port));
        //static public readonly InputSource PS2KEYBOARD = new InputSource("ps2keyboard", "PC PS/2 Keyboard", true, false, false, false, port => new SerialControllerReader3, (port, PS2Keyboard.ReadFromPacket));
        //static public readonly InputSource XBOX = new InputSource("xbox", "Microsoft Xbox", false, true, controllerId => new XboxReader(int.Parse3, (controllerId)));

        public static readonly IReadOnlyList<InputSource> ALL = new List<InputSource> {
#pragma warning disable CS8604 // Possible null reference argument.
            MISTER, POCKET, POCKET_II, CLASSIC, DRIVINGCONTROLLER, ATARIKEYBOARD, PADDLES, ATARI5200, JAGUAR, ATARIVCS_II, ATARIVCS, PIPPIN, EVERCADE, EVERCADE_II, COLECOVISION, CDTV, CD32, C64MINI, C64MINI_II, A500MINI, A500MINI_II, FMTOWNS, INTELLIVISION, DOLPHIN, XBOX, XBOX_II, XBOX360, XBOX360_II, TG16, PCFX, TG16MINI, TG16_II, NES, SNES, VIRTUALBOY, N64, GAMECUBE, WII, SWITCH, SWITCH_II, THREEDO, PC360, PAD, PCKEYBOARD, CDI, SEGA, SATURN3D, DREAMCAST, GENMINI, GENMINI1_II, GENMINI2, GENMINI2_II, NEOGEO, NEOGEOMINI_II, NEOGEOMINI, PLAYSTATION2, PS3_II, PS3, PS4_II, PS4, PS4CRONUS, PS4USB, PSCLASSIC, PSCLASSIC_II, NUON, VSMILE, VFLASH, USBMOUSE
#pragma warning restore CS8604 // Possible null reference argument.
        };

        public static readonly IReadOnlyList<InputSource> ALL_LINUX = new List<InputSource> {
#pragma warning disable CS8604 // Possible null reference argument.
            MISTER, POCKET, POCKET_II, CLASSIC, DRIVINGCONTROLLER, ATARIKEYBOARD, PADDLES, ATARI5200, JAGUAR, ATARIVCS_II, ATARIVCS, PIPPIN, EVERCADE, EVERCADE_II, COLECOVISION, CDTV, CD32, C64MINI, C64MINI_II, A500MINI, A500MINI_II, FMTOWNS, INTELLIVISION, LINUX, LINUXKEY, DOLPHIN, XBOX, XBOX_II, XBOX360, XBOX360_II, TG16, PCFX, TG16MINI, TG16_II, NES, SNES, VIRTUALBOY, N64, GAMECUBE, WII, SWITCH, SWITCH_II, THREEDO, CDI, SEGA, SATURN3D, DREAMCAST, GENMINI, GENMINI1_II, GENMINI2, GENMINI2_II, NEOGEO, NEOGEOMINI, NEOGEOMINI_II, PLAYSTATION2,PS3_II, PS3, PS4, PS4_II, PS4CRONUS, PS4USB, PSCLASSIC, PSCLASSIC_II, NUON, VSMILE, VFLASH
#pragma warning restore CS8604 // Possible null reference argument.
        };

        public static readonly IReadOnlyList<InputSource> ALL_MACOS = new List<InputSource> {
            MISTER, POCKET, POCKET_II, CLASSIC, DRIVINGCONTROLLER, ATARIKEYBOARD, PADDLES, ATARI5200, JAGUAR, ATARIVCS_II, ATARIVCS, PIPPIN, EVERCADE, EVERCADE_II, COLECOVISION, CDTV, CD32, C64MINI, C64MINI_II, A500MINI, A500MINI_II, FMTOWNS, INTELLIVISION, DOLPHIN, XBOX, XBOX_II, XBOX360, XBOX360_II, TG16, PCFX, TG16MINI, TG16_II, NES, SNES, VIRTUALBOY, N64, GAMECUBE, WII, SWITCH, SWITCH_II, THREEDO, CDI, SEGA, SATURN3D, DREAMCAST, GENMINI, GENMINI1_II, GENMINI2, GENMINI2_II,  NEOGEO, NEOGEOMINI, NEOGEOMINI_II, PLAYSTATION2, PS3_II, PS3, PS4_II, PS4, PS4CRONUS, PS4USB, PSCLASSIC, PSCLASSIC_II, NUON, VSMILE, VFLASH
        };

        public static IReadOnlyList<InputSource> GetAllSources()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return ALL_LINUX;
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return ALL_MACOS;
            else
                return ALL;
        }

        public static readonly InputSource DEFAULT = NES;

        public string TypeTag { get; private set; }
        public string Name { get; private set; }
        public bool RequiresComPort { get; private set; }
        public bool RequiresComPort2 { get; private set; }
        public bool RequiresId { get; private set; }
        public bool RequiresHostname { get; private set; }
        public bool RequiresMisterId { get; private set; }

        public int UseUSB2 { get; private set; }

        public Func<string?, bool, IControllerReader?>? BuildReader { get; private set; }

        public Func<string?, string?, bool, IControllerReader?>? BuildReader2 { get; private set; }

        public IControllerReader? BuildReader3 { get; private set; }

        public Func<string, string, string, IControllerReader?>? BuildReader4 { get; private set; }
        public Func<string, string, string, string, IControllerReader?>? BuildReader5 { get; private set; }

        private InputSource(string typeTag, string name, bool requiresComPort, bool requiresId, bool requiresHostname, bool requiresComPort2, bool requiresMisterControllerId, int useUSB2, Func<string?, bool, IControllerReader?>? buildReader)
        {
            TypeTag = typeTag;
            Name = name;
            RequiresComPort = requiresComPort;
            RequiresComPort2 = requiresComPort2;
            RequiresId = requiresId;
            RequiresHostname = requiresHostname;
            RequiresMisterId = requiresMisterControllerId;
            UseUSB2 = useUSB2;
            BuildReader = buildReader;
        }

        private InputSource(string typeTag, string name, bool requiresComPort, bool requiresId, bool requiresHostname, bool requiresComPort2, bool requiresMisterControllerId, int useUSB2, Func<string?, string?, bool, IControllerReader?> buildReader2)
        {
            TypeTag = typeTag;
            Name = name;
            RequiresComPort = requiresComPort;
            RequiresComPort2 = requiresComPort2;
            RequiresId = requiresId;
            RequiresHostname = requiresHostname;
            RequiresMisterId = requiresMisterControllerId;
            UseUSB2 = useUSB2;
            BuildReader2 = buildReader2;
        }

        private InputSource(string typeTag, string name, bool requiresComPort, bool requiresId, bool requiresHostname, bool requiresComPort2, bool requiresMisterControllerId, int useUSB2, IControllerReader? buildReader3)
        {
            TypeTag = typeTag;
            Name = name;
            RequiresComPort = requiresComPort;
            RequiresComPort2 = requiresComPort2;
            RequiresId = requiresId;
            RequiresHostname = requiresHostname;
            RequiresMisterId = requiresMisterControllerId;
            UseUSB2 = useUSB2;
            BuildReader3 = buildReader3;
        }

        private InputSource(string typeTag, string name, bool requiresComPort, bool requiresId, bool requiresHostname, bool requiresComPort2, bool requiresMisterControllerId, int useUSB2, Func<string, string, string, IControllerReader?>? buildReader4)
        { 
            TypeTag = typeTag;
            Name = name;
            RequiresComPort = requiresComPort;
            RequiresComPort2 = requiresComPort2;
            RequiresId = requiresId;
            RequiresHostname = requiresHostname;
            RequiresMisterId = requiresMisterControllerId;
            UseUSB2 = useUSB2;
            BuildReader4 = buildReader4;
        }

        private InputSource(string typeTag, string name, bool requiresComPort, bool requiresId, bool requiresHostname, bool requiresComPort2, bool requiresMisterControllerId, int useUSB2, Func<string, string, string, string, IControllerReader?>? buildReader5)
        {
            TypeTag = typeTag;
            Name = name;
            RequiresComPort = requiresComPort;
            RequiresComPort2 = requiresComPort2;
            RequiresId = requiresId;
            RequiresHostname = requiresHostname;
            RequiresMisterId = requiresMisterControllerId;
            UseUSB2 = useUSB2;
            BuildReader5 = buildReader5;
        }

    }
}