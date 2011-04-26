// Guids.cs
// MUST match guids.h
using System;

namespace IAmyMyOwnOrg.Sudoku
{
    static class GuidList
    {
        public const string guidSudokuPkgString = "ec3ff85c-f49e-457f-8e4c-e78d15adb95f";
        public const string guidSudokuCmdSetString = "3b5e51e2-c803-44df-9ff0-3b1a7e861383";
        public const string guidToolWindowPersistanceString = "dae80dae-96cb-4dd1-8a46-82c17ba40e04";

        public static readonly Guid guidSudokuCmdSet = new Guid(guidSudokuCmdSetString);
    };
}