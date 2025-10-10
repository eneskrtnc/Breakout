// Assets/Scripts/Data/GameDatabase/DatabaseHealth.cs
using System.Collections.Generic;

namespace SpaceTrader.Data
{
    public enum DatabaseState
    {
        NotStarted,
        LoadedPartial,
        LoadedOk,
        Failed,
    }

    public class ValidationProblem
    {
        public string Id; // örn. starter_ship
        public string Domain; // örn. ships
        public string Message; // sorun açıklaması
        public bool IsError; // true=hata, false=uyarı
    }

    public class DatabaseHealth
    {
        public DatabaseState State;
        public int DomainsLoaded; // örn. Ships=1
        public int ItemsLoaded; // toplam def sayısı
        public int Warnings;
        public int Errors;
        public List<ValidationProblem> Problems = new();

        public override string ToString() =>
            $"State={State} Domains={DomainsLoaded} Items={ItemsLoaded} Warn={Warnings} Err={Errors}";
    }
}
