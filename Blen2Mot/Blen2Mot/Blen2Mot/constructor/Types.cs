﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Security.Cryptography;

public class Header
{
    //These ones are just for exporting
    public byte type;
    public sbyte bone;
    public ushort p, dp, m0, dm0, m1, dm1;
    public keyframe[] curves;
    public float[] oP, oM0, oM1;
    public int[] absoluteTime;
    public float miP, maP;



    //These ones are just for importing
    public ushort keyframeCount;
    public uint usesRootSpace;
    public uint adress;

    public void calculateHeaderValues()
    {
        float minP = float.MaxValue,
        maxP = float.MinValue,
        minM0 = float.MaxValue,
        maxM0 = float.MinValue,
        minM1 = float.MaxValue,
        maxM1 = float.MinValue;
        for (int i = 0; i < oP.Length; i++)
        {
            if (Program.IKBones.Length > 0)
            {
                int match = 0;
                foreach (var ik in Program.IKBones)
                {
                    if (ik == bone - 2)
                    {
                        match++;
                    }
                }
                if (match < 1)
                {
                    switch (type)
                    {
                        case 16:
                            oP[i] += Program.bones[bone + 1].pos.X; break;
                        case 17:
                            oP[i] += Program.bones[bone + 1].pos.Y; break;
                        case 18:
                            oP[i] += Program.bones[bone + 1].pos.Z; break;
                        case 80:
                            oP[i] += Program.bones[bone + 1].pos.X; break;
                        case 81:
                            oP[i] += Program.bones[bone + 1].pos.Y; break;
                        case 82:
                            oP[i] += Program.bones[bone + 1].pos.Z; break;
                    } //applies the offset
                }
                else
                {
                    switch (type)
                    {
                        case 16:
                            oP[i] += Program.bones[0].pos.X; break;
                        case 17:
                            oP[i] += Program.bones[0].pos.Y; break;
                        case 18:
                            oP[i] += Program.bones[0].pos.Z; break;
                        case 80:
                            oP[i] += Program.bones[0].pos.X; break;
                        case 81:
                            oP[i] += Program.bones[0].pos.Y; break;
                        case 82:
                            oP[i] += Program.bones[0].pos.Z; break;
                    } //applies the offset
                }
            }
        }
        List<float> vl = oP.ToList(); vl.Sort((y, w) => y.CompareTo(w));
        minP = vl[0]; maxP = vl[vl.Count - 1];

        List<float> m0vl = oM0.ToList(); m0vl.Sort((y, w) => y.CompareTo(w));
        minM0 = m0vl[0]; maxM0 = m0vl[m0vl.Count - 1];

        List<float> m1vl = oM1.ToList(); m1vl.Sort((y, w) => y.CompareTo(w));
        minM1 = m1vl[0]; maxM1 = m1vl[m1vl.Count - 1];



        List<keyframe> curve = new List<keyframe>();
        if (type < 80)
        {
            float rP = minP;
            float rdP = (maxP - rP) / 255;
            p = IEEEBinary16.ToUshort(rP);
            dp = IEEEBinary16.ToUshort(rdP);
            float rM0 = minM0;
            float rdM0 = (maxM0 - rM0) / 255;
            m0 = IEEEBinary16.ToUshort(rM0);
            dm0 = IEEEBinary16.ToUshort(rdM0);
            float rM1 = minM1;
            float rdM1 = (maxM1 - rM1) / 255;
            m1 = IEEEBinary16.ToUshort(rM1);
            dm1 = IEEEBinary16.ToUshort(rdM1);

            miP = rP;
            maP = rdP;
            for (int i = 0; i < oP.Length; i++)
            {
                byte cp = (byte)GlobalTools.Map(oP[i], minP, maxP, 0, 255);
                byte cm0 = (byte)GlobalTools.Map(oM0[i], minM0, maxM0, 0, 255);
                byte cm1 = (byte)GlobalTools.Map(oM1[i], minM1, maxM1, 0, 255);
                byte relative_time = 0;
                if (i - 1 > -1)
                    relative_time = (byte)(absoluteTime[i] - absoluteTime[i - 1]);
                else
                    relative_time = (byte)(absoluteTime[i]);
                curve.Add(new keyframe(cp, cm0, cm1, relative_time, 0, 0, 0, 0));
            }
        }
        else
        {
            p = 0;
            dp = 0;
            m0 = 0;
            dm0 = 0;
            m1 = 0;
            dm1 = 0;
            for (int i = 0; i < oP.Length; i++)
            {
                curve.Add(new keyframe(0, 0, 0, 0, oP[i], oM0[i], oM1[i], (ushort)absoluteTime[i]));
            }
        }
        curves = curve.ToArray();

    }
    public void recalculateType()
    {
        int equalsTo = 1;
        for (int c = 0; c < oP.Length - 1; c++) if (oP[c + 1] == oP[c]) equalsTo++; else equalsTo--; ;
        if (equalsTo > 0)
        {
            switch (type)
            {
                case 16:
                    type = 0;
                    oP[0] += Program.bones[bone + 1].pos.X; break;
                case 17:
                    type = 1;
                    oP[0] += Program.bones[bone + 1].pos.Y; break;
                case 18:
                    type = 2;
                    oP[0] += Program.bones[bone + 1].pos.Z; break;
                case 19:
                    type = 3;
                    break;
                case 20:
                    type = 4;
                    break;
                case 21:
                    type = 5;
                    break;



                case 80:
                    type = 0;
                    oP[0] += Program.bones[bone + 1].pos.X; break;
                case 81:
                    type = 1;
                    oP[0] += Program.bones[bone + 1].pos.Y; break;
                case 82:
                    type = 2;
                    oP[0] += Program.bones[bone + 1].pos.Z; break;
                case 83:
                    type = 3;
                    break;
                case 84:
                    type = 4;
                    break;
                case 85:
                    type = 5;
                    break;
            }
        }
    }
}
public class keyframe
{
    public byte cp, cm0, cm1, time;

    public float p, m0, m1;
    public ushort ABTime;
    public keyframe(byte cp, byte cm0, byte cm1, byte time, float p, float m0, float m1, ushort abTime)
    {
        this.cp = cp;
        this.cm0 = cm0;
        this.cm1 = cm1;
        this.time = time;

        this.p = p;
        this.m0 = m0;
        this.m1 = m1;
        this.ABTime = abTime;
    }
}

public class BONES
{
    public Vector3 pos;
    public short parentingOrder;
    public short id;
}