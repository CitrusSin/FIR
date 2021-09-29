using System;
using static FIR.FIRGameState;

namespace FIR
{
    public class FIRMachinePlayer : FIRPlayer
    {
        protected enum PlayingState
        {
            DEFENDING, ATTACKING
        }

        protected struct AttackPlan
        {
            public Slot firstSlot;
            public int direction;
        }

        protected PlayingState playingState = PlayingState.ATTACKING;
        protected AttackPlan attackPlan = new AttackPlan();
        protected Random random = new Random();

        protected SlotState selfState = SlotState.BLACK;

        public override void SetColor(SlotState color)
        {
            selfState = color;
        }

        public override Slot GetFirstPiece(FIRGameState state)
        {
            Slot s = new Slot(state.Width / 2 + random.Next(-3, 3), state.Height / 2 + random.Next(-3, 3));
            attackPlan.firstSlot = s;
            attackPlan.direction = random.Next(7);
            return s;
        }

        public override Slot GetNextPiece(FIRGameState gameState, Slot eslot)
        {
            SlotState enemyState = (selfState == SlotState.BLACK) ? SlotState.WHITE : SlotState.BLACK;
            Slot enemySlot = eslot;
            Slot downSlot = null;
            
            switch (playingState)
            {
                case PlayingState.ATTACKING:
                    {
                        {
                            bool shouldDefend = false;
                            for (int x=0;x<gameState.Width&&!shouldDefend;x++)
                            {
                                for (int y=0;y<gameState.Height&&!shouldDefend;y++)
                                {
                                    if (gameState[x, y] == enemyState)
                                    {
                                        for (int d=0;d<8&&!shouldDefend;d++)
                                        {
                                            int revd = (d + 4) % 8;
                                            int enemyCount = 0;
                                            Slot s = new Slot(x, y);
                                            for (int t = 0; t < 5; t++)
                                            {
                                                if (gameState.CheckSlot(s))
                                                {
                                                    var downs = new Slot(x, y).GetNextAtDirection(revd);
                                                    if (gameState[s] == enemyState)
                                                    {
                                                        enemyCount++;
                                                    }
                                                    else if (enemyCount == 4 && gameState[s] == SlotState.EMPTY)
                                                    {
                                                        return s;
                                                    }
                                                    else if (enemyCount == 3 && t == 3 && gameState.CheckSlot(downs) && gameState[downs] == SlotState.EMPTY)
                                                    {
                                                        enemySlot = new Slot(x, y);
                                                        shouldDefend = true;
                                                        break;
                                                    }
                                                }
                                                else if (enemyCount == 4)
                                                {
                                                    var downs = new Slot(x, y).GetNextAtDirection(revd);
                                                    if (gameState.CheckSlot(downs))
                                                    {
                                                        return downs;
                                                    }
                                                }
                                                s = s.GetNextAtDirection(d);
                                            }
                                        }
                                    }
                                }
                            }
                            if (shouldDefend)
                            {
                                playingState = PlayingState.DEFENDING;
                                goto case PlayingState.DEFENDING;
                            }
                        }
                        for (int x=0;x<gameState.Width;x++)
                        {
                            for (int y=0;y<gameState.Height;y++)
                            {
                                for (int direction = 0; direction < 8; direction++)
                                {
                                    Slot s = new Slot(x, y);
                                    int selfCount = 0;
                                    for (int t=1;t<=5;t++)
                                    {
                                        if (gameState.CheckSlot(s))
                                        {
                                            if (selfCount == 4 && gameState[s] != enemyState)
                                            {
                                                return s;
                                            }
                                            else if (gameState[s] == selfState)
                                            {
                                                selfCount++;
                                            }
                                            else if (gameState[s] == enemyState)
                                            {
                                                break;
                                            }
                                        }
                                        else break;
                                        s = s.GetNextAtDirection(direction);
                                    }
                                }
                            }
                        }
                        if (attackPlan.firstSlot == null)
                        {
                            int direction = random.Next(7);
                            attackPlan.direction = direction;
                            Slot first = new Slot(gameState.Width / 2, gameState.Height / 2);
                            while (gameState.CheckSlot(first) && gameState[first] != SlotState.EMPTY)
                            {
                                first = first.GetNextAtDirection(direction);
                            }
                            attackPlan.firstSlot = first;
                        }
                        bool needChangePlan = false;
                        {
                            Slot enumSlot = attackPlan.firstSlot;
                            for (int i = 0; i < 5; i++)
                            {
                                if (!gameState.CheckSlot(enumSlot) || gameState[enumSlot] == enemyState)
                                {
                                    needChangePlan = true;
                                    break;
                                }
                                enumSlot = enumSlot.GetNextAtDirection(attackPlan.direction);
                            }
                        }
                        if (needChangePlan)
                        {
                            int comboCount = 0;
                            AttackPlan bestPlan = new AttackPlan
                            {
                                direction = 0,
                                firstSlot = new Slot(0, 0)
                            };
                            for (int x = 0; x < gameState.Width; x++)
                            {
                                for (int y = 0; y < gameState.Height; y++)
                                {
                                    for (int direction = 0; direction < 8; direction++)
                                    {
                                        int count = 0;
                                        Slot s = new Slot(x, y);
                                        for (int i = 0; i < 5; i++)
                                        {
                                            if (!gameState.CheckSlot(s))
                                            {
                                                count = 0;
                                                break;
                                            }
                                            if (gameState[s] == selfState)
                                            {
                                                count++;
                                            }
                                            else if (gameState[s] == enemyState)
                                            {
                                                count = 0;
                                                break;
                                            }
                                            s = s.GetNextAtDirection(direction);
                                        }
                                        if (count >= comboCount)
                                        {
                                            comboCount = count;
                                            bestPlan.direction = direction;
                                            bestPlan.firstSlot = new Slot(x, y);
                                        }
                                    }
                                }
                            }
                            Slot another = bestPlan.firstSlot.GetTimesAtDirection(bestPlan.direction, 5);
                            int revDir = (bestPlan.direction + 4) % 8;
                            if (gameState.CheckSlot(another))
                            {
                                while (gameState[another] != SlotState.EMPTY)
                                {
                                    another = another.GetNextAtDirection(revDir);
                                }
                                if ((another.X - (gameState.Width / 2)) + (another.Y - (gameState.Height / 2)) < (bestPlan.firstSlot.X - (gameState.Width / 2)) + (bestPlan.firstSlot.Y - (gameState.Height / 2)))
                                {
                                    bestPlan.firstSlot = another;
                                    bestPlan.direction = revDir;
                                }
                            }
                            attackPlan = bestPlan;
                        }
                        Slot slot = attackPlan.firstSlot;
                        while (gameState.CheckSlot(slot) && gameState[slot] != SlotState.EMPTY)
                        {
                            slot = slot.GetNextAtDirection(attackPlan.direction);
                        }
                        downSlot = slot;
                    }
                    break;
                case PlayingState.DEFENDING:
                    {
                        int maxCount = 0;
                        bool check1, check2, check3, check4;
                        {
                            // Check at back slash line
                            {
                                int count = 1;
                                Slot emu = enemySlot.UpperLeft;
                                while (gameState.CheckSlot(emu))
                                {
                                    if (gameState[emu] == enemyState)
                                    {
                                        count++;
                                    }
                                    else if (gameState[emu] == selfState) break;
                                    emu = emu.UpperLeft;
                                }
                                emu = enemySlot.BottomRight;
                                while (gameState.CheckSlot(emu))
                                {
                                    if (gameState[emu] == enemyState)
                                    {
                                        count++;
                                    }
                                    else if (gameState[emu] == selfState) break;
                                    emu = emu.BottomRight;
                                }
                                check1 = (count < 3);
                                maxCount = Math.Max(maxCount, count);
                            }
                            // Check at vertical line
                            {
                                int count = 1;
                                Slot emu = enemySlot.Upper;
                                while (gameState.CheckSlot(emu))
                                {
                                    if (gameState[emu] == enemyState)
                                    {
                                        count++;
                                    }
                                    else if (gameState[emu] == selfState) break;
                                    emu = emu.Upper;
                                }
                                emu = enemySlot.Bottom;
                                while (gameState.CheckSlot(emu))
                                {
                                    if (gameState[emu] == enemyState)
                                    {
                                        count++;
                                    }
                                    else if (gameState[emu] == selfState) break;
                                    emu = emu.Bottom;
                                }
                                check2 = (count < 3);
                                maxCount = Math.Max(maxCount, count);
                            }
                            // Check at forward slash line
                            {
                                int count = 1;
                                Slot emu = enemySlot.UpperRight;
                                while (gameState.CheckSlot(emu))
                                {
                                    if (gameState[emu] == enemyState)
                                    {
                                        count++;
                                    }
                                    else if (gameState[emu] == selfState) break;
                                    emu = emu.UpperRight;
                                }
                                emu = enemySlot.BottomLeft;
                                while (gameState.CheckSlot(emu))
                                {
                                    if (gameState[emu] == enemyState)
                                    {
                                        count++;
                                    }
                                    else if (gameState[emu] == selfState) break;
                                    emu = emu.BottomLeft;
                                }
                                check3 = (count < 3);
                                maxCount = Math.Max(maxCount, count);
                            }
                            // Check at horizontal line
                            {
                                int count = 1;
                                Slot emu = enemySlot.Left;
                                while (gameState.CheckSlot(emu))
                                {
                                    if (gameState[emu] == enemyState)
                                    {
                                        count++;
                                    }
                                    else if (gameState[emu] == selfState) break;
                                    emu = emu.Left;
                                }
                                emu = enemySlot.Right;
                                while (gameState.CheckSlot(emu))
                                {
                                    if (gameState[emu] == enemyState)
                                    {
                                        count++;
                                    }
                                    else if (gameState[emu] == selfState) break;
                                    emu = emu.Right;
                                }
                                check4 = (count < 3);
                                maxCount = Math.Max(maxCount, count);
                            }
                            if (check1 && check2 && check3 && check4)
                            {
                                playingState = PlayingState.ATTACKING;
                                goto case PlayingState.ATTACKING;
                            }
                        }
                        if (maxCount < 4)
                        {
                            // Find threat point
                            int threatCount = 0;
                            bool canThreaten = false;
                            Slot threatSlot = null;
                            for (int x = 0; x < gameState.Width; x++)
                            {
                                for (int y = 0; y < gameState.Height; y++)
                                {
                                    for (int direction = 0; direction < 8; direction++)
                                    {
                                        int nowCount = 0;
                                        Slot s = new Slot(x, y);
                                        if (gameState[s] == selfState)
                                        {
                                            for (int t = 0; t < 5; t++)
                                            {
                                                Slot tmp = s.GetTimesAtDirection(direction, t);
                                                if (gameState.CheckSlot(tmp))
                                                {
                                                    if (gameState[tmp] == selfState)
                                                    {
                                                        nowCount++;
                                                    }
                                                    else if (gameState[tmp] == enemyState)
                                                    {
                                                        nowCount = 0;
                                                        break;
                                                    }
                                                }
                                                else
                                                {
                                                    nowCount = 0;
                                                    break;
                                                }
                                            }
                                            if (nowCount >= 3 && nowCount > threatCount)
                                            {
                                                Slot tmp = s;
                                                while (gameState[tmp] != SlotState.EMPTY)
                                                {
                                                    tmp = tmp.GetNextAtDirection(direction);
                                                }
                                                threatCount = nowCount;
                                                threatSlot = tmp;
                                                canThreaten = true;
                                            }
                                        }
                                    }
                                }
                            }
                            if (canThreaten)
                            {
                                downSlot = threatSlot;
                                break;
                            }
                        }
                        if (!check1)
                        {
                            Slot s = enemySlot;
                            while (gameState.CheckSlot(s) && gameState[s] == enemyState)
                            {
                                s = s.UpperLeft;
                            }
                            if (gameState.CheckSlot(s) && gameState[s] == SlotState.EMPTY)
                            {
                                downSlot = s;
                                break;
                            }
                            s = enemySlot;
                            while (gameState.CheckSlot(s) && gameState[s] == enemyState)
                            {
                                s = s.BottomRight;
                            }
                            if (gameState.CheckSlot(s) && gameState[s] == SlotState.EMPTY)
                            {
                                downSlot = s;
                                break;
                            }
                        }
                        else if (!check2)
                        {
                            Slot s = enemySlot;
                            while (gameState.CheckSlot(s) && gameState[s] == enemyState)
                            {
                                s = s.Upper;
                            }
                            if (gameState.CheckSlot(s) && gameState[s] == SlotState.EMPTY)
                            {
                                downSlot = s;
                                break;
                            }
                            s = enemySlot;
                            while (gameState.CheckSlot(s) && gameState[s] == enemyState)
                            {
                                s = s.Bottom;
                            }
                            if (gameState.CheckSlot(s) && gameState[s] == SlotState.EMPTY)
                            {
                                downSlot = s;
                                break;
                            }
                        }
                        else if (!check3)
                        {
                            Slot s = enemySlot;
                            while (gameState.CheckSlot(s) && gameState[s] == enemyState)
                            {
                                s = s.UpperRight;
                            }
                            if (gameState.CheckSlot(s) && gameState[s] == SlotState.EMPTY)
                            {
                                downSlot = s;
                                break;
                            }
                            s = enemySlot;
                            while (gameState.CheckSlot(s) && gameState[s] == enemyState)
                            {
                                s = s.BottomLeft;
                            }
                            if (gameState.CheckSlot(s) && gameState[s] == SlotState.EMPTY)
                            {
                                downSlot = s;
                                break;
                            }
                        }
                        else if (!check4)
                        {
                            Slot s = enemySlot;
                            while (gameState.CheckSlot(s) && gameState[s] == enemyState)
                            {
                                s = s.Left;
                            }
                            if (gameState.CheckSlot(s) && gameState[s] == SlotState.EMPTY)
                            {
                                downSlot = s;
                                break;
                            }
                            s = enemySlot;
                            while (gameState.CheckSlot(s) && gameState[s] == enemyState)
                            {
                                s = s.Right;
                            }
                            if (gameState.CheckSlot(s) && gameState[s] == SlotState.EMPTY)
                            {
                                downSlot = s;
                                break;
                            }
                        }
                    }
                    break;
            }
            if (downSlot != null && !gameState.CheckSlot(downSlot))
            {
                return new Slot((downSlot.X >= gameState.Width || downSlot.X < 0) ? gameState.Width - 1 : downSlot.X, (downSlot.Y >= gameState.Height || downSlot.Y < 0) ? gameState.Height - 1 : downSlot.Y); ;
            }
            return downSlot;
        }
    }
}
