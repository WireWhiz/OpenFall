using Unity.Networking.Transport;
using Unity.NetCode;
using Unity.Mathematics;

public struct PilotSnapshotData : ISnapshotData<PilotSnapshotData>
{
    public uint tick;
    private int PilotDataPlayerId;
    private int PilotMovementSystemDatavelocityX;
    private int PilotMovementSystemDatavelocityY;
    private int PilotMovementSystemDatavelocityZ;
    private uint PilotMovementSystemDataonGround;
    private uint PilotMovementSystemDatajumpCooldown;
    private int PilotMovementSystemDatamovementMode;
    private int RotationValueX;
    private int RotationValueY;
    private int RotationValueZ;
    private int RotationValueW;
    private int TranslationValueX;
    private int TranslationValueY;
    private int TranslationValueZ;
    uint changeMask0;

    public uint Tick => tick;
    public int GetPilotDataPlayerId(GhostDeserializerState deserializerState)
    {
        return (int)PilotDataPlayerId;
    }
    public int GetPilotDataPlayerId()
    {
        return (int)PilotDataPlayerId;
    }
    public void SetPilotDataPlayerId(int val, GhostSerializerState serializerState)
    {
        PilotDataPlayerId = (int)val;
    }
    public void SetPilotDataPlayerId(int val)
    {
        PilotDataPlayerId = (int)val;
    }
    public float3 GetPilotMovementSystemDatavelocity(GhostDeserializerState deserializerState)
    {
        return GetPilotMovementSystemDatavelocity();
    }
    public float3 GetPilotMovementSystemDatavelocity()
    {
        return new float3(PilotMovementSystemDatavelocityX * 0.001f, PilotMovementSystemDatavelocityY * 0.001f, PilotMovementSystemDatavelocityZ * 0.001f);
    }
    public void SetPilotMovementSystemDatavelocity(float3 val, GhostSerializerState serializerState)
    {
        SetPilotMovementSystemDatavelocity(val);
    }
    public void SetPilotMovementSystemDatavelocity(float3 val)
    {
        PilotMovementSystemDatavelocityX = (int)(val.x * 1000);
        PilotMovementSystemDatavelocityY = (int)(val.y * 1000);
        PilotMovementSystemDatavelocityZ = (int)(val.z * 1000);
    }
    public bool GetPilotMovementSystemDataonGround(GhostDeserializerState deserializerState)
    {
        return PilotMovementSystemDataonGround!=0;
    }
    public bool GetPilotMovementSystemDataonGround()
    {
        return PilotMovementSystemDataonGround!=0;
    }
    public void SetPilotMovementSystemDataonGround(bool val, GhostSerializerState serializerState)
    {
        PilotMovementSystemDataonGround = val?1u:0;
    }
    public void SetPilotMovementSystemDataonGround(bool val)
    {
        PilotMovementSystemDataonGround = val?1u:0;
    }
    public bool GetPilotMovementSystemDatajumpCooldown(GhostDeserializerState deserializerState)
    {
        return PilotMovementSystemDatajumpCooldown!=0;
    }
    public bool GetPilotMovementSystemDatajumpCooldown()
    {
        return PilotMovementSystemDatajumpCooldown!=0;
    }
    public void SetPilotMovementSystemDatajumpCooldown(bool val, GhostSerializerState serializerState)
    {
        PilotMovementSystemDatajumpCooldown = val?1u:0;
    }
    public void SetPilotMovementSystemDatajumpCooldown(bool val)
    {
        PilotMovementSystemDatajumpCooldown = val?1u:0;
    }
    public PilotMovementSystemData.MovementMode GetPilotMovementSystemDatamovementMode(GhostDeserializerState deserializerState)
    {
        return (PilotMovementSystemData.MovementMode)PilotMovementSystemDatamovementMode;
    }
    public PilotMovementSystemData.MovementMode GetPilotMovementSystemDatamovementMode()
    {
        return (PilotMovementSystemData.MovementMode)PilotMovementSystemDatamovementMode;
    }
    public void SetPilotMovementSystemDatamovementMode(PilotMovementSystemData.MovementMode val, GhostSerializerState serializerState)
    {
        PilotMovementSystemDatamovementMode = (int)val;
    }
    public void SetPilotMovementSystemDatamovementMode(PilotMovementSystemData.MovementMode val)
    {
        PilotMovementSystemDatamovementMode = (int)val;
    }
    public quaternion GetRotationValue(GhostDeserializerState deserializerState)
    {
        return GetRotationValue();
    }
    public quaternion GetRotationValue()
    {
        return new quaternion(RotationValueX * 0.001f, RotationValueY * 0.001f, RotationValueZ * 0.001f, RotationValueW * 0.001f);
    }
    public void SetRotationValue(quaternion q, GhostSerializerState serializerState)
    {
        SetRotationValue(q);
    }
    public void SetRotationValue(quaternion q)
    {
        RotationValueX = (int)(q.value.x * 1000);
        RotationValueY = (int)(q.value.y * 1000);
        RotationValueZ = (int)(q.value.z * 1000);
        RotationValueW = (int)(q.value.w * 1000);
    }
    public float3 GetTranslationValue(GhostDeserializerState deserializerState)
    {
        return GetTranslationValue();
    }
    public float3 GetTranslationValue()
    {
        return new float3(TranslationValueX * 0.01f, TranslationValueY * 0.01f, TranslationValueZ * 0.01f);
    }
    public void SetTranslationValue(float3 val, GhostSerializerState serializerState)
    {
        SetTranslationValue(val);
    }
    public void SetTranslationValue(float3 val)
    {
        TranslationValueX = (int)(val.x * 100);
        TranslationValueY = (int)(val.y * 100);
        TranslationValueZ = (int)(val.z * 100);
    }

    public void PredictDelta(uint tick, ref PilotSnapshotData baseline1, ref PilotSnapshotData baseline2)
    {
        var predictor = new GhostDeltaPredictor(tick, this.tick, baseline1.tick, baseline2.tick);
        PilotDataPlayerId = predictor.PredictInt(PilotDataPlayerId, baseline1.PilotDataPlayerId, baseline2.PilotDataPlayerId);
        PilotMovementSystemDatavelocityX = predictor.PredictInt(PilotMovementSystemDatavelocityX, baseline1.PilotMovementSystemDatavelocityX, baseline2.PilotMovementSystemDatavelocityX);
        PilotMovementSystemDatavelocityY = predictor.PredictInt(PilotMovementSystemDatavelocityY, baseline1.PilotMovementSystemDatavelocityY, baseline2.PilotMovementSystemDatavelocityY);
        PilotMovementSystemDatavelocityZ = predictor.PredictInt(PilotMovementSystemDatavelocityZ, baseline1.PilotMovementSystemDatavelocityZ, baseline2.PilotMovementSystemDatavelocityZ);
        PilotMovementSystemDataonGround = (uint)predictor.PredictInt((int)PilotMovementSystemDataonGround, (int)baseline1.PilotMovementSystemDataonGround, (int)baseline2.PilotMovementSystemDataonGround);
        PilotMovementSystemDatajumpCooldown = (uint)predictor.PredictInt((int)PilotMovementSystemDatajumpCooldown, (int)baseline1.PilotMovementSystemDatajumpCooldown, (int)baseline2.PilotMovementSystemDatajumpCooldown);
        PilotMovementSystemDatamovementMode = predictor.PredictInt(PilotMovementSystemDatamovementMode, baseline1.PilotMovementSystemDatamovementMode, baseline2.PilotMovementSystemDatamovementMode);
        RotationValueX = predictor.PredictInt(RotationValueX, baseline1.RotationValueX, baseline2.RotationValueX);
        RotationValueY = predictor.PredictInt(RotationValueY, baseline1.RotationValueY, baseline2.RotationValueY);
        RotationValueZ = predictor.PredictInt(RotationValueZ, baseline1.RotationValueZ, baseline2.RotationValueZ);
        RotationValueW = predictor.PredictInt(RotationValueW, baseline1.RotationValueW, baseline2.RotationValueW);
        TranslationValueX = predictor.PredictInt(TranslationValueX, baseline1.TranslationValueX, baseline2.TranslationValueX);
        TranslationValueY = predictor.PredictInt(TranslationValueY, baseline1.TranslationValueY, baseline2.TranslationValueY);
        TranslationValueZ = predictor.PredictInt(TranslationValueZ, baseline1.TranslationValueZ, baseline2.TranslationValueZ);
    }

    public void Serialize(int networkId, ref PilotSnapshotData baseline, ref DataStreamWriter writer, NetworkCompressionModel compressionModel)
    {
        changeMask0 = (PilotDataPlayerId != baseline.PilotDataPlayerId) ? 1u : 0;
        changeMask0 |= (PilotMovementSystemDatavelocityX != baseline.PilotMovementSystemDatavelocityX ||
                                           PilotMovementSystemDatavelocityY != baseline.PilotMovementSystemDatavelocityY ||
                                           PilotMovementSystemDatavelocityZ != baseline.PilotMovementSystemDatavelocityZ) ? (1u<<1) : 0;
        changeMask0 |= (PilotMovementSystemDataonGround != baseline.PilotMovementSystemDataonGround) ? (1u<<2) : 0;
        changeMask0 |= (PilotMovementSystemDatajumpCooldown != baseline.PilotMovementSystemDatajumpCooldown) ? (1u<<3) : 0;
        changeMask0 |= (PilotMovementSystemDatamovementMode != baseline.PilotMovementSystemDatamovementMode) ? (1u<<4) : 0;
        changeMask0 |= (RotationValueX != baseline.RotationValueX ||
                                           RotationValueY != baseline.RotationValueY ||
                                           RotationValueZ != baseline.RotationValueZ ||
                                           RotationValueW != baseline.RotationValueW) ? (1u<<5) : 0;
        changeMask0 |= (TranslationValueX != baseline.TranslationValueX ||
                                           TranslationValueY != baseline.TranslationValueY ||
                                           TranslationValueZ != baseline.TranslationValueZ) ? (1u<<6) : 0;
        writer.WritePackedUIntDelta(changeMask0, baseline.changeMask0, compressionModel);
        bool isPredicted = GetPilotDataPlayerId() == networkId;
        writer.WritePackedUInt(isPredicted?1u:0, compressionModel);
        if ((changeMask0 & (1 << 0)) != 0)
            writer.WritePackedIntDelta(PilotDataPlayerId, baseline.PilotDataPlayerId, compressionModel);
        if ((changeMask0 & (1 << 5)) != 0)
        {
            writer.WritePackedIntDelta(RotationValueX, baseline.RotationValueX, compressionModel);
            writer.WritePackedIntDelta(RotationValueY, baseline.RotationValueY, compressionModel);
            writer.WritePackedIntDelta(RotationValueZ, baseline.RotationValueZ, compressionModel);
            writer.WritePackedIntDelta(RotationValueW, baseline.RotationValueW, compressionModel);
        }
        if ((changeMask0 & (1 << 6)) != 0)
        {
            writer.WritePackedIntDelta(TranslationValueX, baseline.TranslationValueX, compressionModel);
            writer.WritePackedIntDelta(TranslationValueY, baseline.TranslationValueY, compressionModel);
            writer.WritePackedIntDelta(TranslationValueZ, baseline.TranslationValueZ, compressionModel);
        }
        if (isPredicted)
        {
            if ((changeMask0 & (1 << 1)) != 0)
            {
                writer.WritePackedIntDelta(PilotMovementSystemDatavelocityX, baseline.PilotMovementSystemDatavelocityX, compressionModel);
                writer.WritePackedIntDelta(PilotMovementSystemDatavelocityY, baseline.PilotMovementSystemDatavelocityY, compressionModel);
                writer.WritePackedIntDelta(PilotMovementSystemDatavelocityZ, baseline.PilotMovementSystemDatavelocityZ, compressionModel);
            }
            if ((changeMask0 & (1 << 2)) != 0)
                writer.WritePackedUIntDelta(PilotMovementSystemDataonGround, baseline.PilotMovementSystemDataonGround, compressionModel);
            if ((changeMask0 & (1 << 3)) != 0)
                writer.WritePackedUIntDelta(PilotMovementSystemDatajumpCooldown, baseline.PilotMovementSystemDatajumpCooldown, compressionModel);
            if ((changeMask0 & (1 << 4)) != 0)
                writer.WritePackedIntDelta(PilotMovementSystemDatamovementMode, baseline.PilotMovementSystemDatamovementMode, compressionModel);
        }
    }

    public void Deserialize(uint tick, ref PilotSnapshotData baseline, ref DataStreamReader reader,
        NetworkCompressionModel compressionModel)
    {
        this.tick = tick;
        changeMask0 = reader.ReadPackedUIntDelta(baseline.changeMask0, compressionModel);
        bool isPredicted = reader.ReadPackedUInt(compressionModel)!=0;
        if ((changeMask0 & (1 << 0)) != 0)
            PilotDataPlayerId = reader.ReadPackedIntDelta(baseline.PilotDataPlayerId, compressionModel);
        else
            PilotDataPlayerId = baseline.PilotDataPlayerId;
        if ((changeMask0 & (1 << 5)) != 0)
        {
            RotationValueX = reader.ReadPackedIntDelta(baseline.RotationValueX, compressionModel);
            RotationValueY = reader.ReadPackedIntDelta(baseline.RotationValueY, compressionModel);
            RotationValueZ = reader.ReadPackedIntDelta(baseline.RotationValueZ, compressionModel);
            RotationValueW = reader.ReadPackedIntDelta(baseline.RotationValueW, compressionModel);
        }
        else
        {
            RotationValueX = baseline.RotationValueX;
            RotationValueY = baseline.RotationValueY;
            RotationValueZ = baseline.RotationValueZ;
            RotationValueW = baseline.RotationValueW;
        }
        if ((changeMask0 & (1 << 6)) != 0)
        {
            TranslationValueX = reader.ReadPackedIntDelta(baseline.TranslationValueX, compressionModel);
            TranslationValueY = reader.ReadPackedIntDelta(baseline.TranslationValueY, compressionModel);
            TranslationValueZ = reader.ReadPackedIntDelta(baseline.TranslationValueZ, compressionModel);
        }
        else
        {
            TranslationValueX = baseline.TranslationValueX;
            TranslationValueY = baseline.TranslationValueY;
            TranslationValueZ = baseline.TranslationValueZ;
        }
        if (isPredicted)
        {
            if ((changeMask0 & (1 << 1)) != 0)
            {
                PilotMovementSystemDatavelocityX = reader.ReadPackedIntDelta(baseline.PilotMovementSystemDatavelocityX, compressionModel);
                PilotMovementSystemDatavelocityY = reader.ReadPackedIntDelta(baseline.PilotMovementSystemDatavelocityY, compressionModel);
                PilotMovementSystemDatavelocityZ = reader.ReadPackedIntDelta(baseline.PilotMovementSystemDatavelocityZ, compressionModel);
            }
            else
            {
                PilotMovementSystemDatavelocityX = baseline.PilotMovementSystemDatavelocityX;
                PilotMovementSystemDatavelocityY = baseline.PilotMovementSystemDatavelocityY;
                PilotMovementSystemDatavelocityZ = baseline.PilotMovementSystemDatavelocityZ;
            }
            if ((changeMask0 & (1 << 2)) != 0)
                PilotMovementSystemDataonGround = reader.ReadPackedUIntDelta(baseline.PilotMovementSystemDataonGround, compressionModel);
            else
                PilotMovementSystemDataonGround = baseline.PilotMovementSystemDataonGround;
            if ((changeMask0 & (1 << 3)) != 0)
                PilotMovementSystemDatajumpCooldown = reader.ReadPackedUIntDelta(baseline.PilotMovementSystemDatajumpCooldown, compressionModel);
            else
                PilotMovementSystemDatajumpCooldown = baseline.PilotMovementSystemDatajumpCooldown;
            if ((changeMask0 & (1 << 4)) != 0)
                PilotMovementSystemDatamovementMode = reader.ReadPackedIntDelta(baseline.PilotMovementSystemDatamovementMode, compressionModel);
            else
                PilotMovementSystemDatamovementMode = baseline.PilotMovementSystemDatamovementMode;
        }
    }
    public void Interpolate(ref PilotSnapshotData target, float factor)
    {
        SetRotationValue(math.slerp(GetRotationValue(), target.GetRotationValue(), factor));
        SetTranslationValue(math.lerp(GetTranslationValue(), target.GetTranslationValue(), factor));
    }
}
