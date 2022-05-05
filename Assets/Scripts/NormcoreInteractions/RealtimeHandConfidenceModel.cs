using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;
using Normal.Realtime.Serialization;

[RealtimeModel]
public partial class RealtimeHandConfidenceModel
{
    [RealtimeProperty(1, true, true)]
    private int _trackingConfidence;

    [RealtimeProperty(2, true, false)]
    private int _clientID;

    [RealtimeProperty(3, true, false)]
    private int _handType;
}

/* ----- Begin Normal Autogenerated Code ----- */
public partial class RealtimeHandConfidenceModel : RealtimeModel {
    public int trackingConfidence {
        get {
            return _cache.LookForValueInCache(_trackingConfidence, entry => entry.trackingConfidenceSet, entry => entry.trackingConfidence);
        }
        set {
            if (this.trackingConfidence == value) return;
            _cache.UpdateLocalCache(entry => { entry.trackingConfidenceSet = true; entry.trackingConfidence = value; return entry; });
            InvalidateReliableLength();
            FireTrackingConfidenceDidChange(value);
        }
    }
    
    public int clientID {
        get {
            return _cache.LookForValueInCache(_clientID, entry => entry.clientIDSet, entry => entry.clientID);
        }
        set {
            if (this.clientID == value) return;
            _cache.UpdateLocalCache(entry => { entry.clientIDSet = true; entry.clientID = value; return entry; });
            InvalidateReliableLength();
        }
    }
    
    public int handType {
        get {
            return _cache.LookForValueInCache(_handType, entry => entry.handTypeSet, entry => entry.handType);
        }
        set {
            if (this.handType == value) return;
            _cache.UpdateLocalCache(entry => { entry.handTypeSet = true; entry.handType = value; return entry; });
            InvalidateReliableLength();
        }
    }
    
    public delegate void PropertyChangedHandler<in T>(RealtimeHandConfidenceModel model, T value);
    public event PropertyChangedHandler<int> trackingConfidenceDidChange;
    
    private struct LocalCacheEntry {
        public bool trackingConfidenceSet;
        public int trackingConfidence;
        public bool clientIDSet;
        public int clientID;
        public bool handTypeSet;
        public int handType;
    }
    
    private LocalChangeCache<LocalCacheEntry> _cache = new LocalChangeCache<LocalCacheEntry>();
    
    public enum PropertyID : uint {
        TrackingConfidence = 1,
        ClientID = 2,
        HandType = 3,
    }
    
    public RealtimeHandConfidenceModel() : this(null) {
    }
    
    public RealtimeHandConfidenceModel(RealtimeModel parent) : base(null, parent) {
    }
    
    protected override void OnParentReplaced(RealtimeModel previousParent, RealtimeModel currentParent) {
        UnsubscribeClearCacheCallback();
    }
    
    private void FireTrackingConfidenceDidChange(int value) {
        try {
            trackingConfidenceDidChange?.Invoke(this, value);
        } catch (System.Exception exception) {
            UnityEngine.Debug.LogException(exception);
        }
    }
    
    protected override int WriteLength(StreamContext context) {
        int length = 0;
        if (context.fullModel) {
            FlattenCache();
            length += WriteStream.WriteVarint32Length((uint)PropertyID.TrackingConfidence, (uint)_trackingConfidence);
            length += WriteStream.WriteVarint32Length((uint)PropertyID.ClientID, (uint)_clientID);
            length += WriteStream.WriteVarint32Length((uint)PropertyID.HandType, (uint)_handType);
        } else if (context.reliableChannel) {
            LocalCacheEntry entry = _cache.localCache;
            if (entry.trackingConfidenceSet) {
                length += WriteStream.WriteVarint32Length((uint)PropertyID.TrackingConfidence, (uint)entry.trackingConfidence);
            }
            if (entry.clientIDSet) {
                length += WriteStream.WriteVarint32Length((uint)PropertyID.ClientID, (uint)entry.clientID);
            }
            if (entry.handTypeSet) {
                length += WriteStream.WriteVarint32Length((uint)PropertyID.HandType, (uint)entry.handType);
            }
        }
        return length;
    }
    
    protected override void Write(WriteStream stream, StreamContext context) {
        var didWriteProperties = false;
        
        if (context.fullModel) {
            stream.WriteVarint32((uint)PropertyID.TrackingConfidence, (uint)_trackingConfidence);
            stream.WriteVarint32((uint)PropertyID.ClientID, (uint)_clientID);
            stream.WriteVarint32((uint)PropertyID.HandType, (uint)_handType);
        } else if (context.reliableChannel) {
            LocalCacheEntry entry = _cache.localCache;
            if (entry.trackingConfidenceSet || entry.clientIDSet || entry.handTypeSet) {
                _cache.PushLocalCacheToInflight(context.updateID);
                ClearCacheOnStreamCallback(context);
            }
            if (entry.trackingConfidenceSet) {
                stream.WriteVarint32((uint)PropertyID.TrackingConfidence, (uint)entry.trackingConfidence);
                didWriteProperties = true;
            }
            if (entry.clientIDSet) {
                stream.WriteVarint32((uint)PropertyID.ClientID, (uint)entry.clientID);
                didWriteProperties = true;
            }
            if (entry.handTypeSet) {
                stream.WriteVarint32((uint)PropertyID.HandType, (uint)entry.handType);
                didWriteProperties = true;
            }
            
            if (didWriteProperties) InvalidateReliableLength();
        }
    }
    
    protected override void Read(ReadStream stream, StreamContext context) {
        while (stream.ReadNextPropertyID(out uint propertyID)) {
            switch (propertyID) {
                case (uint)PropertyID.TrackingConfidence: {
                    int previousValue = _trackingConfidence;
                    _trackingConfidence = (int)stream.ReadVarint32();
                    bool trackingConfidenceExistsInChangeCache = _cache.ValueExistsInCache(entry => entry.trackingConfidenceSet);
                    if (!trackingConfidenceExistsInChangeCache && _trackingConfidence != previousValue) {
                        FireTrackingConfidenceDidChange(_trackingConfidence);
                    }
                    break;
                }
                case (uint)PropertyID.ClientID: {
                    _clientID = (int)stream.ReadVarint32();
                    break;
                }
                case (uint)PropertyID.HandType: {
                    _handType = (int)stream.ReadVarint32();
                    break;
                }
                default: {
                    stream.SkipProperty();
                    break;
                }
            }
        }
    }
    
    #region Cache Operations
    
    private StreamEventDispatcher _streamEventDispatcher;
    
    private void FlattenCache() {
        _trackingConfidence = trackingConfidence;
        _clientID = clientID;
        _handType = handType;
        _cache.Clear();
    }
    
    private void ClearCache(uint updateID) {
        _cache.RemoveUpdateFromInflight(updateID);
    }
    
    private void ClearCacheOnStreamCallback(StreamContext context) {
        if (_streamEventDispatcher != context.dispatcher) {
            UnsubscribeClearCacheCallback(); // unsub from previous dispatcher
        }
        _streamEventDispatcher = context.dispatcher;
        _streamEventDispatcher.AddStreamCallback(context.updateID, ClearCache);
    }
    
    private void UnsubscribeClearCacheCallback() {
        if (_streamEventDispatcher != null) {
            _streamEventDispatcher.RemoveStreamCallback(ClearCache);
            _streamEventDispatcher = null;
        }
    }
    
    #endregion
}
/* ----- End Normal Autogenerated Code ----- */
