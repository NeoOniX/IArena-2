using System;
using System.Collections.Generic;
using System.Text;

public class Logs{

    private struct LogStruct{
        public string time;
        public string txt;

        public string T (){
            return time + ": " + txt; 
        }
    }

    public bool printInConsole = false;

    private List<LogStruct> logs = new List<LogStruct>();

    public void Log(string txt){
        LogStruct l = new LogStruct();
        l.txt = txt;
        l.time = DateTime.Now.ToString();
        if (printInConsole){
            UnityEngine.Debug.Log(txt);
        }
    }

    public string GetLogEntry(int index){
        if (index >= 0 && index < logs.Count){
            return logs[index].T();
        }else{
            return "";
        }
    }

    public override string ToString()
    {
        StringBuilder s = new StringBuilder("");
        for (int i =0;i<logs.Count;i++){
            s.Append(GetLogEntry(i));
            s.AppendLine();
        }
        return s.ToString();
    }
}