﻿using Steamworks;
using System;
using System.Collections;
using UnityEngine;

namespace CDOverhaul.Workshop
{
    public class OverhaulWorkshopItem : IDisposable
    {
        private bool m_IsDisposed;
        public bool IsDisposed() => m_IsDisposed;

        void IDisposable.Dispose()
        {
            GC.SuppressFinalize(this);
            m_IsDisposed = true;
            Name = null;
        }
        ~OverhaulWorkshopItem()
        {
            ((IDisposable)this).Dispose();
        }

        public OverhaulWorkshopItem(SteamUGCDetails_t details)
        {
            Name = details.m_rgchTitle;
            Description = details.m_rgchDescription;
            ID = details.m_nPublishedFileId;
            UpVotesCount = (int)details.m_unVotesUp;
            DownVotesCount = (int)details.m_unVotesDown;
            Stars = Mathf.Min((details.m_flScore * 5f) + 1f, 5f);
        }

        public PublishedFileId_t ID;
        public string Name;
        public string Description;

        public int UpVotesCount;
        public int DownVotesCount;
        public int TotalVotes => UpVotesCount + DownVotesCount;
        public float Stars;

        public string ThumbnailURL;
    }
}