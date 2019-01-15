using UnityEngine;

public static class ViconVRPN
{
    public static Vector3 vrpnTrackerPos(string address, int channel)
    {
        var vector = VRPN.vrpnTrackerPos(address, channel);
        return new Vector3(vector.x, vector.z, vector.y);
    }

    public static Quaternion vrpnTrackerQuat(string address, int channel)
    {
        var quaternion = VRPN.vrpnTrackerQuat(address, channel);
        return new Quaternion(-quaternion.x, -quaternion.z, -quaternion.y, quaternion.w);
        //Quaternion quaternion = VRPN.vrpnTrackerQuat(address, channel);

        //if (quaternion == new Quaternion(-505, -505, -505, -505))
        //    return Quaternion.identity;
        ////Vector3 euler = quaternion.eulerAngles;

        //quaternion = ConvertRightHandedToLeftHandedQuaternion(quaternion);
        //Vector3 euler = quaternion2Euler(quaternion, RotSeq.zyx);
        ////quaternion = ConvertToRightHand(euler);
        ////euler = new Vector3(euler.x, euler.y, euler.z);

        ////quaternion = Quaternion.Euler(euler);
        ////quaternion = ConvertRightHandedToLeftHandedQuaternion(quaternion);

        ////euler = Quaternion.ToEulerAngles(quaternion)*Mathf.Rad2Deg;
        ////euler = new Vector3(euler.x, euler.z, euler.y);
        ////quaternion = Quaternion.Euler(euler);

        //return Quaternion.Euler(-euler.x, -euler.y, -euler.z);


        //return quaternion;
        //return new Quaternion(-quaternion.x, -quaternion.z, -quaternion.y, quaternion.w);
    }

    private static Quaternion ConvertRightHandedToLeftHandedQuaternion(Quaternion rightHandedQuaternion)
    {
        return new Quaternion(-rightHandedQuaternion.x,
                               -rightHandedQuaternion.z,
                               -rightHandedQuaternion.y,
                                 rightHandedQuaternion.w);
    }

    static Quaternion ConvertToRightHand(Vector3 Euler)
    {
        Quaternion x = Quaternion.AngleAxis(Euler.x, Vector3.right);
        Quaternion y = Quaternion.AngleAxis(-Euler.y, Vector3.up);
        Quaternion z = Quaternion.AngleAxis(-Euler.z, Vector3.forward);
        return (z * y * x);
    }

    enum RotSeq
    {
        zyx, zyz, zxy, zxz, yxz, yxy, yzx, yzy, xyz, xyx, xzy, xzx
    };

    private static Vector3 Twoaxisrot(float r11, float r12, float r21, float r31, float r32)
    {
        Vector3 ret = new Vector3();
        ret.x = Mathf.Rad2Deg * Mathf.Atan2(r11, r12);
        ret.y = Mathf.Rad2Deg * Mathf.Acos(r21);
        ret.z = Mathf.Rad2Deg * Mathf.Atan2(r31, r32);
        return ret;
    }

    private static Vector3 Threeaxisrot(float r11, float r12, float r21, float r31, float r32)
    {
        Vector3 ret = new Vector3();
        ret.x = Mathf.Rad2Deg*Mathf.Atan2(r31, r32);
        ret.y = Mathf.Rad2Deg * Mathf.Asin(r21);
        ret.z = Mathf.Rad2Deg * Mathf.Atan2(r11, r12);
        return ret;
    }

    private static Vector3 quaternion2Euler(Quaternion q, RotSeq rotSeq)
    {
        switch (rotSeq)
        {
            case RotSeq.zyx:
                return Threeaxisrot(2 * (q.x * q.y + q.w * q.z),
                    q.w * q.w + q.x * q.x - q.y * q.y - q.z * q.z,
                    -2 * (q.x * q.z - q.w * q.y),
                    2 * (q.y * q.z + q.w * q.x),
                    q.w * q.w - q.x * q.x - q.y * q.y + q.z * q.z);


            case RotSeq.zyz:
                return Twoaxisrot(2 * (q.y * q.z - q.w * q.x),
                    2 * (q.x * q.z + q.w * q.y),
                    q.w * q.w - q.x * q.x - q.y * q.y + q.z * q.z,
                    2 * (q.y * q.z + q.w * q.x),
                    -2 * (q.x * q.z - q.w * q.y));


            case RotSeq.zxy:
                return Threeaxisrot(-2 * (q.x * q.y - q.w * q.z),
                    q.w * q.w - q.x * q.x + q.y * q.y - q.z * q.z,
                    2 * (q.y * q.z + q.w * q.x),
                    -2 * (q.x * q.z - q.w * q.y),
                    q.w * q.w - q.x * q.x - q.y * q.y + q.z * q.z);


            case RotSeq.zxz:
                return Twoaxisrot(2 * (q.x * q.z + q.w * q.y),
                    -2 * (q.y * q.z - q.w * q.x),
                    q.w * q.w - q.x * q.x - q.y * q.y + q.z * q.z,
                    2 * (q.x * q.z - q.w * q.y),
                    2 * (q.y * q.z + q.w * q.x));


            case RotSeq.yxz:
                return Threeaxisrot(2 * (q.x * q.z + q.w * q.y),
                    q.w * q.w - q.x * q.x - q.y * q.y + q.z * q.z,
                    -2 * (q.y * q.z - q.w * q.x),
                    2 * (q.x * q.y + q.w * q.z),
                    q.w * q.w - q.x * q.x + q.y * q.y - q.z * q.z);

            case RotSeq.yxy:
                return Twoaxisrot(2 * (q.x * q.y - q.w * q.z),
                    2 * (q.y * q.z + q.w * q.x),
                    q.w * q.w - q.x * q.x + q.y * q.y - q.z * q.z,
                    2 * (q.x * q.y + q.w * q.z),
                    -2 * (q.y * q.z - q.w * q.x));


            case RotSeq.yzx:
                
                return Threeaxisrot(-2 * (q.x * q.z - q.w * q.y),
                    q.w * q.w + q.x * q.x - q.y * q.y - q.z * q.z,
                    2 * (q.x * q.y + q.w * q.z),
                    -2 * (q.y * q.z - q.w * q.x),
                    q.w * q.w - q.x * q.x + q.y * q.y - q.z * q.z);


            case RotSeq.yzy:
                return Twoaxisrot(2 * (q.y * q.z + q.w * q.x),
                    -2 * (q.x * q.y - q.w * q.z),
                    q.w * q.w - q.x * q.x + q.y * q.y - q.z * q.z,
                    2 * (q.y * q.z - q.w * q.x),
                    2 * (q.x * q.y + q.w * q.z));


            case RotSeq.xyz:
                return Threeaxisrot(-2 * (q.y * q.z - q.w * q.x),
                    q.w * q.w - q.x * q.x - q.y * q.y + q.z * q.z,
                    2 * (q.x * q.z + q.w * q.y),
                    -2 * (q.x * q.y - q.w * q.z),
                    q.w * q.w + q.x * q.x - q.y * q.y - q.z * q.z);


            case RotSeq.xyx:
                return Twoaxisrot(2 * (q.x * q.y + q.w * q.z),
                    -2 * (q.x * q.z - q.w * q.y),
                    q.w * q.w + q.x * q.x - q.y * q.y - q.z * q.z,
                    2 * (q.x * q.y - q.w * q.z),
                    2 * (q.x * q.z + q.w * q.y));


            case RotSeq.xzy:
                return Threeaxisrot(2 * (q.y * q.z + q.w * q.x),
                    q.w * q.w - q.x * q.x + q.y * q.y - q.z * q.z,
                    -2 * (q.x * q.y - q.w * q.z),
                    2 * (q.x * q.z + q.w * q.y),
                    q.w * q.w + q.x * q.x - q.y * q.y - q.z * q.z);


            case RotSeq.xzx:
                return Twoaxisrot(2 * (q.x * q.z - q.w * q.y),
                    2 * (q.x * q.y + q.w * q.z),
                    q.w * q.w + q.x * q.x - q.y * q.y - q.z * q.z,
                    2 * (q.x * q.z + q.w * q.y),
                    -2 * (q.x * q.y - q.w * q.z));

            default:
                Debug.LogError("No good sequence");
                return Vector3.zero;

        }

    }

}
