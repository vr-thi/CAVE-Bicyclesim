using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CONST {
	public static string getANGLE() { return "GETADC 1"; }
	public static string getSPEED() { return "GETADC 2"; }
	public static string getSTATE() { return "GETSTATUS"; }

	public static string setPORT(int aPortNbr) { 
		if (aPortNbr > 0 && aPortNbr < 9) 
			return "SETPORT " + aPortNbr + "."; 
		return "";
	}

	public static string getPORT(int aPortNbr) { 
		if (aPortNbr > 0 && aPortNbr < 4) 
			return "GETPORT " + aPortNbr; 
		return "";
	}
}
