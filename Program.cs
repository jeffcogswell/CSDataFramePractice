#nullable disable
using System.Threading.Tasks.Dataflow;
using System.Collections.Generic;
using System.Xml.XPath;

namespace csops2;

public enum DataOp {
    none,
    lessthan,
    greaterthan
}

public enum ListType {
    none,
    op,
    searchfield,
    setfield,
    debug1,
    debug2
}

/* public class InfoOLD {
    public int id { get; set; }
    public int size { get; set; }
    public string word { get; set; }
    public string another { get; set ;}
}*/

public class Info : Dictionary<string, object> {
    public override string ToString()
    {
        string result = "";

        // Old, hard way... rewrite with LINQ
        foreach (string key in this.Keys)
        {
            result += $"[{key}]={this[key]} ";
        }
        return result;
    }
}

// This version won't use a templated class
// Also later we can add other OpValue types.
public class DataList {
    public List<Info> TheList = null;
    public DataList Parent = null;

    public ListType TheListType = ListType.none;

    public DataOp Op = DataOp.none;
    public int OpValue = 0; // For use when TheLisType is ListType.op.
    public string Field = ""; // For use in indexer when TheListType is ListType.searchfield or setfield

    public DataList() {
        TheList = new List<Info>();
    }

    public DataList(List<Info> start) {
        TheList = new List<Info>(start);
    }
    public DataList(DataList start) {
        Parent = start;
    }

    // Indexes are for field names -- in this case id, size, word, another (and that's probably why they're not using classes in Python pandas!!)

    public DataList this[string field] {
        get {
            DataList nextone = new DataList(this);
            nextone.Field = field;
            nextone.TheListType = ListType.searchfield;
            return nextone;
        }
    }

    public static DataList operator <(DataList dl, int max) {
        DataList nextone = new DataList(dl);
        nextone.Op = DataOp.lessthan;
        nextone.OpValue = max;
        nextone.TheListType = ListType.op;
        return nextone;
    }

    public static DataList operator >(DataList dl, int max) {
        DataList nextone = new DataList(dl);
        nextone.Op = DataOp.greaterthan;
        nextone.OpValue = max;
        nextone.TheListType = ListType.op;
        return nextone;
    }

    public string GetSearchField() {
        // We need to climb up the ladder until we find a field.
        if (TheListType == ListType.searchfield) {
            return Field;
        }
        else if (Parent != null) {
            return Parent.GetSearchField();
        }
        else {
            return "";
        }
    }

    public List<Info> GetList() {
        // If Parent is null, return the list filtered for the operation.
        List<Info> UseList = TheList;
        // If Parent is not null, ask it for its list, and then filter with the operation.
        if (Parent != null) {
            UseList = Parent.GetList();
        }

        // Now we in theory should have the list. Next, we need to know which field we're searching for.
        // That should come from an ancestor. If not present, we need to throw an exception I guess.

        string field = GetSearchField();
        if (field == "") {
            return UseList;
        }

        if (Op == DataOp.lessthan) {
            // For now just cast...
            // We probably need to build our own packed structure type
            return UseList.FindAll(item => (int)item[field] < OpValue);
        }
        else if (Op == DataOp.greaterthan) {
            return UseList.FindAll(item => (int)item[field] > OpValue);
        }
        else {
            return UseList;                
        }        

    }

    // TODO: Do we really need the DataList parameter?
    // The original code I was looking at did...
    //    df.loc[ df['SomeField] > 70, 'AnotherField'   ] = 'NewValue';
    public DataList loc(DataList start, string fieldname) {
        DataList next = new DataList(start);
        next.TheListType = ListType.setfield;
        next.Field = fieldname;
        return next;
    }

    // Now to make the assignment work, we need to do a bunch of user-defined conversion operators...
    // See: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/assignment-operator#operator-overloadability
    // See: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/user-defined-conversion-operators
    // UPDATE: This won't work since C# doesn't let us overload the equal operator.
    // So for this step we just need to do a function.
    /*public static implicit operator DataList(string str) {
        DataList result = new DataList();
        result.TheListType = ListType.debug1;
        return result;
    }*/

    public DataList update(Object value) {
        // Only allow this if ListType is setfield
        if (TheListType != ListType.setfield) {
            throw new Exception("Can only call update after loc, providing a field name.");
        }

        // Grab the list, and update all the items for the field
        GetList().ForEach(item => item[Field] = value);
        return this;
    }

    public override string ToString()
    {

        string result = "";
        foreach (var row in GetList()) {
            result += row.ToString() + "\r\n";
        }
        return result;
    }

}


/*

Old test code
        // womp womp
        //DataList df3 = (DataList)"Hello";
        //Console.WriteLine(df3.TheListType);


        DataList df2 = df["size"];
        Console.WriteLine(df2);

        DataList df3 = df2 < 14;
        Console.WriteLine(df3);

        List<Info> mylist = df3.GetList();
        Console.WriteLine(mylist.Count);

        DataList df2 = df["size"] < 21;
        Console.WriteLine(df2);
        //Console.WriteLine(df2.GetList().Count);


        //DataList df3 = df2.loc(df2, "another");
        //df3.update("tiger");
        //Console.WriteLine(df3);
        //Console.WriteLine(df);

*/

class Program
{
    static void Main(string[] args)
    {

        var MyData = new List<Info>() {
            new Info { {"size", 10}, {"word", "apple"}, {"another", "cat"} },
            new Info { {"size", 25}, {"word", "banana"}, {"another", "dog"} },
            new Info { {"size", 50}, {"word", "cantaloup"}, {"another", "lion"} },
            new Info { {"size", 5}, {"word", "orange"}, {"another", "bear"} },
            new Info { {"size", 30}, {"word", "grape"}, {"another", "zebra"} },
            new Info { {"size", 70}, {"word", "kiwi"}, {"another", "cheetah"} },
            new Info { {"size", 40}, {"word", "plum"}, {"another", "gorilla"} },
            new Info { {"size", 60}, {"word", "pear"}, {"another", "kangaroo"} }
        };

        DataList df = new DataList(MyData);
        Console.WriteLine("Initial data:");
        Console.WriteLine(df);

        df.loc(df["size"] < 50, "another").update("tiger");  // IT WORKS!!!
        Console.WriteLine("Search for records where size is less than 50, and update the 'another' field to 'tiger'");
        Console.WriteLine(df);


        Console.WriteLine("Resetting data...");
        df = new DataList(MyData);

        df.loc(df["size"] > 10 < 60, "another").update("tiger");  // IT WORKS!!!
        Console.WriteLine("Search for records where size is greater than 10 and less than 60, and update the 'another' field to 'tiger'");
        Console.WriteLine(df);
       
        // Next step -- load CSV. Find third party library for that.

    }
}
