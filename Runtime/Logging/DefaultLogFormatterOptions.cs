using System;

namespace Rusleo.Utils.Runtime.Logging
{
    [Serializable]
    public class DefaultLogFormatterOptions
    {
        public bool IncludeTimestamp = true;
        public bool IncludeOwner = true;
        public bool IncludeTags = true;
        public bool IncludeMetadata = true;
        public bool IncludeCorrId = true;
        public bool MultilineException = true;

        // Метаданные
        public int MetadataMaxCount = 8; // лимит ключей
        public string[] MetadataPriorityOrder = { "playerId", "user", "scene", "matchId" }; // сначала важные
        public bool SortRemainingMetaAlphabetically = true;
    }
}