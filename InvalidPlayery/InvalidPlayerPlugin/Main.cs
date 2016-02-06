﻿using JsRuntime.Hosting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace InvalidPlayerPlugin
{
    public class Main

    {
        private static JavaScriptSourceContext currentSourceContext = JavaScriptSourceContext.FromIntPtr(IntPtr.Zero);
        private static JavaScriptRuntime runtime;
        private static Queue taskQueue = new Queue();

        public Main()
        {
        }

        public string init()
        {
            JavaScriptContext context;

            if (Native.JsCreateRuntime(JavaScriptRuntimeAttributes.None, null, out runtime) != JavaScriptErrorCode.NoError)
                return "failed to create runtime.";

            if (Native.JsCreateContext(runtime, out context) != JavaScriptErrorCode.NoError)
                return "failed to create execution context.";

            if (Native.JsSetCurrentContext(context) != JavaScriptErrorCode.NoError)
                return "failed to set current context.";

            // ES6 Promise callback
            JavaScriptPromiseContinuationCallback promiseContinuationCallback = delegate (JavaScriptValue task, IntPtr callbackState)
            {
                taskQueue.Enqueue(task);
            };

            if (Native.JsSetPromiseContinuationCallback(promiseContinuationCallback, IntPtr.Zero) != JavaScriptErrorCode.NoError)
                return "failed to setup callback for ES6 Promise";

            if (Native.JsProjectWinRTNamespace("Windows") != JavaScriptErrorCode.NoError)
                return "failed to project windows namespace.";

            if (Native.JsStartDebugging() != JavaScriptErrorCode.NoError)
                return "failed to start debugging.";

            return "NoError";
        }

            public void UsingWinrt() {
                Native.JsProjectWinRTNamespace("InvalidPlayerCore");
                Native.JsProjectWinRTNamespace("InvalidPlayerCore.Common");
                Native.JsProjectWinRTNamespace("InvalidPlayerCore.Service");
            }

        public void UsingObject() {
            JavaScriptValue _globalObject;
            Native.ThrowIfError(Native.JsGetGlobalObject(out _globalObject));
            //_globalObject.SetProperty()
        }

        JavaScriptValue CallJsFunction(string name, params JavaScriptValue[] parameters)
        {
            JavaScriptValue _globalObject;
            Native.ThrowIfError(Native.JsGetGlobalObject(out _globalObject));
            var function = _globalObject.GetProperty(JavaScriptPropertyId.FromString(name));
            return function.CallFunction(parameters);
        }

        public string runScript(string script)
            {
                IntPtr returnValue;

                try
                {
                    JavaScriptValue result;

                    if (Native.JsRunScript(script, currentSourceContext++, "", out result) != JavaScriptErrorCode.NoError)
                    {
                        // Get error message and clear exception
                        JavaScriptValue exception;
                        if (Native.JsGetAndClearException(out exception) != JavaScriptErrorCode.NoError)
                            return "failed to get and clear exception";

                        JavaScriptPropertyId messageName;
                        if (Native.JsGetPropertyIdFromName("message",
                            out messageName) != JavaScriptErrorCode.NoError)
                            return "failed to get error message id";

                        JavaScriptValue messageValue;
                        if (Native.JsGetProperty(exception, messageName, out messageValue)
                            != JavaScriptErrorCode.NoError)
                            return "failed to get error message";

                        IntPtr message;
                        UIntPtr length;
                        if (Native.JsStringToPointer(messageValue, out message, out length) != JavaScriptErrorCode.NoError)
                            return "failed to convert error message";

                        return Marshal.PtrToStringUni(message);
                    }

                    // Execute promise tasks stored in taskQueue 
                    while (taskQueue.Count != 0)
                    {
                        JavaScriptValue task = (JavaScriptValue)taskQueue.Dequeue();
                        JavaScriptValue promiseResult;
                        JavaScriptValue global;
                        Native.JsGetGlobalObject(out global);
                        JavaScriptValue[] args = new JavaScriptValue[1] { global };
                        Native.JsCallFunction(task, args, 1, out promiseResult);
                    }

                    // Convert the return value.
                    JavaScriptValue stringResult;
                    UIntPtr stringLength;
                    if (Native.JsConvertValueToString(result, out stringResult) != JavaScriptErrorCode.NoError)
                        return "failed to convert value to string.";
                    if (Native.JsStringToPointer(stringResult, out returnValue, out stringLength) != JavaScriptErrorCode.NoError)
                        return "failed to convert return value.";
                }
                catch (Exception e)
                {
                    return "chakrahost: fatal error: internal error: " + e.Message;
                }

                return Marshal.PtrToStringUni(returnValue);
            }
        }
    }