using System;
using System.Collections.Generic;

namespace OverhaulMod.Content.Personalization
{
    public class PersonalizationUserInfo
    {
        public List<string> DiscoveredItems;

        public List<string> FavoriteItems;

        public List<string> UnverifiedItems;

        public Dictionary<string, int> ItemVersions;

        [NonSerialized]
        public bool m_isDirty;

        public void FixValues()
        {
            if (DiscoveredItems == null)
                DiscoveredItems = new List<string>();

            if (DiscoveredItems.Count == 0)
                DiscoverAllItems();

            if (FavoriteItems == null)
                FavoriteItems = new List<string>();

            if (UnverifiedItems == null)
                UnverifiedItems = new List<string>();

            RefreshAllItemsVerification();

            if (ItemVersions == null)
                ItemVersions = new Dictionary<string, int>();

            if (ItemVersions.Count == 0)
                RefreshAllItemVersions();
        }

        public void SetIsDirty(bool value = true)
        {
            m_isDirty = value;
        }

        public bool IsDirty()
        {
            return m_isDirty;
        }

        public void SaveIfDirty()
        {
            if (IsDirty())
            {
                SetIsDirty(false);
                PersonalizationManager.Instance.SaveUserInfo();
            }
        }

        public void DiscoverAllItems()
        {
            foreach (PersonalizationItemInfo item in PersonalizationManager.Instance.itemList.Items)
                SetIsItemDiscovered(item);
        }

        public void SetIsItemDiscovered(PersonalizationItemInfo itemInfo)
        {
            SetIsItemDiscovered(itemInfo.ItemID);
        }

        public void SetIsItemDiscovered(string itemId)
        {
            if (!DiscoveredItems.Contains(itemId))
                DiscoveredItems.Add(itemId);

            SetIsDirty(true);
        }

        public bool IsItemDiscovered(PersonalizationItemInfo itemInfo)
        {
            return IsItemDiscovered(itemInfo.ItemID);
        }

        public bool IsItemDiscovered(string itemId)
        {
            return DiscoveredItems.Contains(itemId);
        }

        public void SetIsItemFavorite(PersonalizationItemInfo itemInfo, bool value)
        {
            SetIsItemFavorite(itemInfo.ItemID, value);
        }

        public void SetIsItemFavorite(string itemId, bool value)
        {
            List<string> list = FavoriteItems;

            if (list.Contains(itemId) && !value)
                _ = list.Remove(itemId);
            else if (!list.Contains(itemId) && value)
                list.Add(itemId);

            SetIsDirty(true);
        }

        public bool IsItemFavorite(PersonalizationItemInfo itemInfo)
        {
            return IsItemFavorite(itemInfo.ItemID);
        }

        public bool IsItemFavorite(string itemId)
        {
            return FavoriteItems.Contains(itemId);
        }

        public void RefreshAllItemsVerification()
        {
            foreach (PersonalizationItemInfo item in PersonalizationManager.Instance.itemList.Items)
                if (!item.IsVerified)
                    SetIsItemUnverified(item, true);
        }

        public void SetIsItemUnverified(PersonalizationItemInfo itemInfo, bool value)
        {
            SetIsItemUnverified(itemInfo.ItemID, value);
        }

        public void SetIsItemUnverified(string itemId, bool value)
        {
            if (value && !UnverifiedItems.Contains(itemId))
                UnverifiedItems.Add(itemId);
            else if (!value)
                _ = UnverifiedItems.Remove(itemId);

            SetIsDirty(true);
        }

        public bool IsItemUnverified(PersonalizationItemInfo itemInfo)
        {
            return IsItemUnverified(itemInfo.ItemID);
        }

        public bool IsItemUnverified(string itemId)
        {
            return UnverifiedItems.Contains(itemId);
        }

        public void RefreshAllItemVersions()
        {
            foreach (PersonalizationItemInfo item in PersonalizationManager.Instance.itemList.Items)
                SetItemVersion(item, item.Version);
        }

        public void SetItemVersion(PersonalizationItemInfo itemInfo, int value)
        {
            SetItemVersion(itemInfo.ItemID, value);
        }

        public void SetItemVersion(string itemId, int value)
        {
            if (value != 0)
            {
                if (ItemVersions.ContainsKey(itemId))
                    ItemVersions[itemId] = value;
                else
                    ItemVersions.Add(itemId, value);
            }
            else
            {
                _ = ItemVersions.Remove(itemId);
            }

            SetIsDirty(true);
        }

        public int GetItemVersion(PersonalizationItemInfo itemInfo)
        {
            return GetItemVersion(itemInfo.ItemID);
        }

        public int GetItemVersion(string itemId)
        {
            if (ItemVersions.ContainsKey(itemId))
                return ItemVersions[itemId];

            return 0;
        }
    }
}
