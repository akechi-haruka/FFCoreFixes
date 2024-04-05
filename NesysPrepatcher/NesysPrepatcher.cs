using System;
using Mono.Cecil;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace NesysPrepatcher
{
    public static class Preloader {

        public static IEnumerable<string> TargetDLLs { get; } = new[] { "Assembly-CSharp.dll" };

        public static void Patch(AssemblyDefinition assembly) {
            if (assembly.Name.Name == "Assembly-CSharp") {
                Trace.TraceInformation("Checking Nesys...");

                string nesys_file = "game_Data\\Plugins\\SimpleNesys.dll";
                string nesys_file_bak = nesys_file + ".bak";
                byte[] amd = File.ReadAllBytes(nesys_file);
                if (amd[0x221C0] != 0x2E) {
                    // D:\ 16-bit
                    PatchOffset(ref amd, 0x221C0, 0x2E, 0x00, 0x5C);
                    // D:\ ASCII
                    PatchOffset(ref amd, 0x22228, 0x2E, 0x5C);
                    PatchOffset(ref amd, 0x22238, 0x2E, 0x5C);
                    PatchOffset(ref amd, 0x22250, 0x2E, 0x5C);
                    PatchOffset(ref amd, 0x22270, 0x2E, 0x5C);
                    PatchOffset(ref amd, 0x222A0, 0x2E, 0x5C);
                    // remove https
                    PatchOffset(ref amd, 0x83B, 0x00);
                    PatchOffset(ref amd, 0x33F1, 0x50, 0x00);
                    PatchOffset(ref amd, 0x36F4, 0x50, 0x00);
                    PatchOffset(ref amd, 0x3449, 0x00);
                    PatchOffset(ref amd, 0x374C, 0x00);
                    // network url
                    PatchOffset(ref amd, 0x22318, "http://nesys.emu");
                    if (!File.Exists(nesys_file_bak)) {
                        File.Copy(nesys_file, nesys_file_bak);
                    }
                    File.WriteAllBytes(nesys_file, amd);
                    Trace.TraceInformation("Successfully patched NESYS dll.");
                }

            }
        }

        private static void PatchOffset(ref byte[] prog, long addr, params byte[] bytes) {
            Trace.TraceInformation("Patching 0x"+addr.ToString("X") + "(len="+bytes.Length+")");
            for (int i = 0; i < bytes.Length; i++) {
                prog[addr + i] = bytes[i];
            }
        }
        
        private static void PatchOffset(ref byte[] prog, long addr, String str) {
            byte[] b = Encoding.ASCII.GetBytes(str);
            Trace.TraceInformation("Patching 0x"+addr.ToString("X") + "(string="+str+",len="+b.Length+")");
            PatchOffset(ref prog, addr, b);
            PatchOffset(ref prog, addr + b.Length, 0x00);
        }
    }

}
