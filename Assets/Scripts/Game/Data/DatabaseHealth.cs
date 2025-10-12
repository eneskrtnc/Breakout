// Assets/Scripts/Data/GameDatabase/DatabaseHealth.cs
using System.Collections.Generic;

namespace SpaceTrader.Game.Data
{
    public enum DatabaseState
    {
        NotStarted = 0,
        Initializing = 1,      // ek: yükleme başlarken
        LoadedPartial = 2,     // bazı domain/öğeler yüklendi
        LoadedOk = 3,          // hepsi başarıyla yüklendi
        Ready = LoadedOk,      // ek: "Ready" ile geriye dönük uyum
        Failed = 4,
    }

    [System.Serializable]
    public class ValidationProblem
    {
        public string Id;        // örn. "starter_ship"
        public string Domain;    // örn. "ships"
        public string Message;   // sorun açıklaması
        public bool IsError;     // true=hata, false=uyarı
    }

    [System.Serializable]
    public class DatabaseHealth
    {
        public DatabaseState State = DatabaseState.NotStarted;
        public int DomainsLoaded;   // örn. Ships=1
        public int ItemsLoaded;     // toplam def sayısı
        public int Warnings;
        public int Errors;
        public List<ValidationProblem> Problems = new();

        public void Reset()
        {
            State = DatabaseState.NotStarted;
            DomainsLoaded = 0;
            ItemsLoaded = 0;
            Warnings = 0;
            Errors = 0;
            Problems.Clear();
        }

        public void StartInit()
        {
            State = DatabaseState.Initializing;
            DomainsLoaded = 0;
            ItemsLoaded = 0;
        }

        public void MarkLoadedOk(int domains, int items)
        {
            State = DatabaseState.LoadedOk; // Ready ile aynı
            DomainsLoaded = domains;
            ItemsLoaded = items;
        }

        public void MarkPartial(int domains, int items)
        {
            State = DatabaseState.LoadedPartial;
            DomainsLoaded = domains;
            ItemsLoaded = items;
        }

        public void MarkFailed(string message = null)
        {
            State = DatabaseState.Failed;
            if (!string.IsNullOrEmpty(message))
                Problems.Add(new ValidationProblem { Message = message, Domain = "-", Id = "-", IsError = true });
            Errors = Problems.FindAll(p => p.IsError).Count;
            Warnings = Problems.FindAll(p => !p.IsError).Count;
        }

        public void AddWarning(string domain, string id, string message)
        {
            Problems.Add(new ValidationProblem { Domain = domain, Id = id, Message = message, IsError = false });
            Warnings++;
        }

        public void AddError(string domain, string id, string message)
        {
            Problems.Add(new ValidationProblem { Domain = domain, Id = id, Message = message, IsError = true });
            Errors++;
        }

        public override string ToString() =>
            $"State={State} Domains={DomainsLoaded} Items={ItemsLoaded} Warn={Warnings} Err={Errors}";
    }
}
