namespace Helios
{
    public class PouchVM
    {
        private string _hashSetKey;
        public int? Id { get; set; }
        public string Pouchid { get; set; }
        public int? Fkbatch { get; set; }
        public int? Pathyear { get; set; }
        public int? Pathmonth { get; set; }       
        public string HastSetKey { get => _hashSetKey; }
        public void SetHashSetKey(string Pouchid, int? Fkbatch, int? Pathmonth, int? Pathyear)
        {
            _hashSetKey = $"{Pouchid}_{Fkbatch}_{Pathmonth}_{Pathyear}";
        }
    }
    public class BathImages
    {

        public string FileName { get; set; }
        public string FileFullName { get; set; }
        public string HashSetKey { get; set; }
        public string Fkbatch { get; set; }

    }

}
