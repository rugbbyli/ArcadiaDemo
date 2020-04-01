using System;
using clojure.lang;
using UnityEngine;
using UnityEngine.UI;

public class Hello : MonoBehaviour
{
    [SerializeField] private Text target;
    
    public void OnClick(Text target)
    {
        target.text = string.Empty;

        TryInvoke(() => Arcadia.Util.require("demo.hello"), "require");
        TryInvoke(() => RT.var("demo.hello", "hello2").invoke(), "invoke demo.hello.hello2");
        TryInvoke(() => RT.var("demo.hello", "hello").invoke("clojure"), "invoke demo.hello.hello");
    }
    
    void TryInvoke(Action invoke, string tag)
    {
        try
        {
            invoke();
            AppendLog($"test {tag} succ");
        }
        catch (Exception e)
        {
            AppendLog($"test {tag} fail: {e.Message}");
            Debug.LogError(e.Message + "\n" + e.StackTrace);
        }
    }

    void TryInvoke<T>(Func<T> invoke, string tag)
    {
        try
        {
            var ret = invoke();
            AppendLog($"test {tag} succ: {ret}");
        }
        catch (Exception e)
        {
            AppendLog($"test {tag} fail: {e.Message}");
        }
    }

    void AppendLog(string log)
    {
        target.text += log;
        target.text += "\n";
    }
}
