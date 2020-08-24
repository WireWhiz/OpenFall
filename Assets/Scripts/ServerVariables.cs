using System;
using System.Collections.Generic;
using System.IO;
using Unity.Collections;
using Unity.Networking.Transport;

public static class ServerVariables
{
    public static PilotSettings pilotSettings;

    private static string[] lines;
    [Serializable]
    public struct PilotSettings
    {
        public Speeds speeds;
        [Serializable]
        public struct Speeds
        {
            public float gravity;

            public float maxGroundSpeed;//our max ground speed
            public float groundAcceleration;
            public float runSpeed;//our max ground speed when moving forwards
            public float minGroundSpeed;//if we are moving at a speed below this we can instantly stop moving

            public float maxSlideSpeed;
            public float slideAcceleration;
            public float minSlideSpeed;

            public float airAcceleration;

            public float maxJumpSpeed;
            public float secondJumpMaxSpeed;
            public float jumpAcceleration;
            public float secondJumpAcceleration;

            public float maxWallSpeed;
            public float wallAcceleration;
            public float wallJumpAcceleration;
        }
        public float groundFriction;
        public float slideFriction;
        public float jumpHeight;
        public float secondJumpHeight;
        public float maxWallTime;

        public float playerRaduis;
        public float headPadding;
        public float maxWalkAngle;

        public override string ToString()
        {
            string output = "";
            var speedVaribles = typeof(PilotSettings.Speeds).GetFields();
            for (int i = 0; i < speedVaribles.Length; i++)
            {
                if (speedVaribles[i].FieldType == typeof(float))
                    output += ((float)typeof(PilotSettings.Speeds).GetField(speedVaribles[i].Name).GetValue(speeds)).ToString()+ " ";
            }
            return output;
        }
        
        public FixedListFloat128 ToFloatArray()
        {
            var varibles = typeof(PilotSettings).GetFields();
            FixedListFloat128 floats = new FixedListFloat128();
            for (int i = 0; i < varibles.Length; i++)
            {
                if (varibles[i].FieldType == typeof(float))
                    floats.Add((float)typeof(PilotSettings).GetField(varibles[i].Name).GetValue(this));
            }
            var speedVaribles = typeof(PilotSettings.Speeds).GetFields();
            for (int i = 0; i < speedVaribles.Length; i++)
            {
                if (speedVaribles[i].FieldType == typeof(float))
                    floats.Add((float)typeof(PilotSettings.Speeds).GetField(speedVaribles[i].Name).GetValue(this.speeds));
            }
            return floats;
        }
        public static PilotSettings FromFloatArray(FixedListFloat128 floatsList)
        {
            PilotSettings serverVariables = default;
            Object settingsObject = serverVariables;
            Object speedSettingsObject = serverVariables.speeds;

            int index = 0;
            float[] floats = floatsList.ToArray();
            var varibles = typeof(PilotSettings).GetFields();
            for (int i = 0; i < varibles.Length; i++)
            {
                if (varibles[i].FieldType == typeof(float))
                {
                    typeof(PilotSettings).GetField(varibles[i].Name).SetValue(settingsObject, floats[index++]);
                }
            }
            var speedVaribles = typeof(PilotSettings.Speeds).GetFields();
            for (int i = 0; i < speedVaribles.Length; i++)
            {
                if (speedVaribles[i].FieldType == typeof(float))
                {
                    typeof(PilotSettings.Speeds).GetField(speedVaribles[i].Name).SetValue(speedSettingsObject, floats[index++]);
                }

            }

            serverVariables = (PilotSettings)settingsObject;
            serverVariables.speeds = (PilotSettings.Speeds)speedSettingsObject;
            return serverVariables;
        }
        
    }
    public static void Serialize(PilotSettings serverVariables, ref DataStreamWriter writer)
    {
        var varibles = typeof(PilotSettings).GetFields();
        for (int i = 0; i < varibles.Length; i++)
        {
            if (varibles[i].FieldType == typeof(float))
                writer.WriteFloat((float)typeof(PilotSettings).GetField(varibles[i].Name).GetValue(serverVariables));
        }
        var speedVaribles = typeof(PilotSettings.Speeds).GetFields();
        for (int i = 0; i < speedVaribles.Length; i++)
        {
            if (speedVaribles[i].FieldType == typeof(float))
                writer.WriteFloat((float)typeof(PilotSettings.Speeds).GetField(speedVaribles[i].Name).GetValue(serverVariables.speeds));
        }

    }
    public static PilotSettings Deserialize(ref DataStreamReader reader)
    {
        PilotSettings serverVariables = default;
        Object settingsObject = serverVariables;
        Object speedSettingsObject = serverVariables.speeds;

        var varibles = typeof(PilotSettings).GetFields();
        for (int i = 0; i < varibles.Length; i++)
        {
            if (varibles[i].FieldType == typeof(float))
                typeof(PilotSettings).GetField(varibles[i].Name).SetValue(settingsObject, reader.ReadFloat());
        }
        var speedVaribles = typeof(PilotSettings.Speeds).GetFields();
        for (int i = 0; i < speedVaribles.Length; i++)
        {
            if (speedVaribles[i].FieldType == typeof(float))
                typeof(PilotSettings.Speeds).GetField(varibles[i].Name).SetValue(speedSettingsObject, reader.ReadFloat());

        }

        serverVariables = (PilotSettings)settingsObject;
        serverVariables.speeds = (PilotSettings.Speeds)speedSettingsObject;
        return serverVariables;

    }
    public static void Quantize(ref PilotSettings serverVariables)
    {
        Object settingsObject = serverVariables;
        Object speedSettingsObject = serverVariables.speeds;

        var varibles = typeof(PilotSettings).GetFields();
        for (int i = 0; i < varibles.Length; i++)
        {
            if (varibles[i].FieldType == typeof(float))
            {
                var field = typeof(PilotSettings).GetField(varibles[i].Name);
                field.SetValue(settingsObject, Quantizer.Quantize((float)field.GetValue(settingsObject), 100));
            }
        }
        var speedVaribles = typeof(PilotSettings.Speeds).GetFields();
        for (int i = 0; i < speedVaribles.Length; i++)
        {
            if (speedVaribles[i].FieldType == typeof(float))
            {
                var field = typeof(PilotSettings.Speeds).GetField(speedVaribles[i].Name);
                field.SetValue(speedSettingsObject, Quantizer.Quantize((float)field.GetValue(speedSettingsObject), 100));
            }
        }

        serverVariables = (PilotSettings)settingsObject;
        serverVariables.speeds = (PilotSettings.Speeds)speedSettingsObject;
    }
    public static PilotSettings GetPlayerMovementValues()
    {
        //Convert our values found in the gauntlet to useable values
        float kphToMps = 1 / 3.6f;

        PilotSettings settings = new PilotSettings
        {
            speeds = new PilotSettings.Speeds
            {
                gravity = 60 * kphToMps,
                
                maxGroundSpeed = 14 * kphToMps,
                runSpeed = 22 * kphToMps,
                groundAcceleration = 12,
                minGroundSpeed = 22 * kphToMps,

                maxSlideSpeed = 32 * kphToMps,
                slideAcceleration = 10 * kphToMps,
                minSlideSpeed = 7 * kphToMps,

                airAcceleration = 2,
                maxJumpSpeed = 8 * kphToMps,
                secondJumpMaxSpeed = 16 * kphToMps,
                jumpAcceleration = 5 * kphToMps,
                secondJumpAcceleration = 34 * kphToMps,

                maxWallSpeed = 30 * kphToMps,
                wallAcceleration = 30 * kphToMps / 1.2f,
                wallJumpAcceleration = 20 * kphToMps,
            },


            groundFriction = 2,
            slideFriction = 2,
            jumpHeight = 1f,
            secondJumpHeight = 1f,

            maxWallTime = 4,
            playerRaduis = .2f,
            headPadding = .05f,
            maxWalkAngle = 45,

        };

        Object settingsObject = settings;
        Object speedSettingsObject = settings.speeds;

        string path = Path.Combine(UnityEngine.Application.streamingAssetsPath, "PlayerMovement.svconfig");
        lines = File.ReadAllLines(path);

        List<string> variableNames = new List<string>();
        foreach(System.Reflection.FieldInfo info in typeof(PilotSettings).GetFields())
        {
            if (info.FieldType == typeof(float))
                variableNames.Add(info.Name);
        }
        List<string> speedVariableNames = new List<string>();
        foreach (System.Reflection.FieldInfo info in typeof(PilotSettings.Speeds).GetFields())
        {
            if (info.FieldType == typeof(float))
                speedVariableNames.Add(info.Name);
        }

        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i] == "")
                continue;
            if (lines[i][0] == '$')
            {
                string line = lines[i].Trim('$');
                string[] elements = line.Split(' ');
                if (elements[0] == "" && !lines[i].Contains(" <- Variable name not recognized"))
                {
                    lines[i] += " <- Variable name not recognized";
                    continue;
                }
                elements[0] = elements[0].ToLower();
                bool varFound = false;
                foreach(string name in variableNames)
                {
                    UnityEngine.Debug.Log("checked name: " + name);
                    if(elements[0] == name.ToLower())
                    {
                        if(SetFloat(elements[1], out float value, i))
                        {
                            UnityEngine.Debug.Log("Wrote " + value + " to " + name);
                            typeof(PilotSettings).GetField(name).SetValue(settingsObject, value);
                            UnityEngine.Debug.Log("Set variable: " + name);
                        }
                        varFound = true;
                        break;
                    }
                }
                if(!varFound)
                    foreach (string name in speedVariableNames)
                    {
                        UnityEngine.Debug.Log("checked name: " + name);
                        if (elements[0] == name.ToLower())
                        {
                            if (SetFloat(elements[1], out float value, i, kphToMps))
                            {
                                typeof(PilotSettings.Speeds).GetField(name).SetValue(speedSettingsObject, value);
                                UnityEngine.Debug.Log(String.Format("Set variable {0} to {1}" , name, value));
                            }
                            varFound = true;
                            break;
                        }
                    }
                if (!varFound && !lines[i].Contains(" <- Variable name not recognized"))
                {
                    lines[i] += " <- Variable name not recognized";
                    continue;
                }
                else if(lines[i].Contains(" <- Variable name not recognized"))
                {
                    lines[i] = lines[i].Trim(" <- Variable name not recognized".ToCharArray());
                }
            }
        }
        File.WriteAllLines(path, lines);
        settings = (PilotSettings)settingsObject;
        settings.speeds = (PilotSettings.Speeds)speedSettingsObject;

        Quantize(ref settings);
        return settings;
    }
    static bool SetFloat(string floatText, out float value, int index, float multiplyBy = 1)
    {
        value = 0;
        if (float.TryParse(floatText, out float parsed))
        {
            value = parsed * multiplyBy;
            if (lines[index].Contains(" <- Could not parse variable"))
            {
                lines[index] += lines[index].Trim(" <- Could not parse variable".ToCharArray());
            }
            return true;
        }
        else if (!lines[index].Contains(" <- Could not parse variable"))
            lines[index] += " <- Could not parse variable";
        return false;
    }
}
