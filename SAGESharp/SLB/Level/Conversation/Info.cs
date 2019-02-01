﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SAGESharp.SLB.Level.Conversation
{
    public class Info : IEquatable<Info>
    {
        public LineSide LineSide { get; set; }

        public uint ConditionStart { get; set; }

        public uint ConditionEnd { get; set; }

        public Identifier StringLabel { get; set; }

        public int StringIndex { get; set; }

        public IList<Frame> Frames { get; set; }

        public bool Equals(Info other)
        {
            if (other == null)
            {
                return false;
            }

            return LineSide == other.LineSide &&
                ConditionStart == other.ConditionStart &&
                ConditionEnd == other.ConditionEnd &&
                StringLabel == other.StringLabel &&
                StringIndex == other.StringIndex &&
                Frames.SafeSequenceEquals(other.Frames);
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();

            result.AppendFormat("LineSide={0}", LineSide).Append(", ");
            result.AppendFormat("ConditionStart={0}", ConditionStart).Append(", ");
            result.AppendFormat("ConditionEnd={0}", ConditionEnd).Append(", ");
            result.AppendFormat("StringLabel={0}", StringLabel).Append(", ");
            result.AppendFormat("StringIndex={0}", StringIndex).Append(", ");
            if (Frames == null)
            {
                result.Append("Frames=null");
            }
            else if (Frames.Count != 0)
            {
                result.AppendFormat("Frames=[({0})]", string.Join("), (", Frames));
            }
            else
            {
                result.Append("Frames=[]");
            }

            return result.ToString();
        }

        public override bool Equals(object other)
        {
            return Equals(other as Info);
        }

        public override int GetHashCode()
        {
            int hash = 17;
            LineSide.AddHashCodeByVal(ref hash, 73);
            ConditionStart.AddHashCodeByVal(ref hash, 73);
            ConditionEnd.AddHashCodeByVal(ref hash, 73);
            StringLabel.AddHashCodeByVal(ref hash, 73);
            StringIndex.AddHashCodeByVal(ref hash, 73);
            Frames.AddHashCodesByRef(ref hash, 73, 37);

            return hash;
        }

        public static bool operator ==(Info left, Info right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }
            else if (left is null)
            {
                return right.Equals(left);
            }
            else
            {
                return left.Equals(right);
            }
        }

        public static bool operator !=(Info left, Info right)
        {
            return !(left == right);
        }
    }
}