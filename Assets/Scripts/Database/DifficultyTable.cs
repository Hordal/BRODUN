// DifficultyTable.cs — 난이도 4단계 (문서 06-6).
using System.Collections.Generic;
using BroDungeon.Data;

namespace BroDungeon.Database
{
    public struct DifficultyConfig
    {
        public float EnemyMul;     // 적 강화 배율
        public float TrapDamageMul;// 함정 피해 배율
        public float ParryWindowMul;
        public string DeathRule;
        public DifficultyConfig(float e, float t, float p, string death)
        { EnemyMul = e; TrapDamageMul = t; ParryWindowMul = p; DeathRule = death; }
    }

    public static class DifficultyTable
    {
        public static readonly Dictionary<Difficulty, DifficultyConfig> All =
            new Dictionary<Difficulty, DifficultyConfig>
        {
            { Difficulty.Story,  new DifficultyConfig(0.5f, 0.3f, 2.0f, "즉시 부활") },
            { Difficulty.Normal, new DifficultyConfig(1.0f, 1.0f, 1.0f, "거점+일부 유실") },
            { Difficulty.Hard,   new DifficultyConfig(1.3f, 1.2f, 0.7f, "거점+유실多") },
            { Difficulty.Trial,  new DifficultyConfig(1.5f, 1.3f, 0.5f, "포션5개, C풀 없음") },
        };

        public static DifficultyConfig Get(Difficulty d) => All[d];
    }
}
