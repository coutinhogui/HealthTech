using System;
using System.Text.Json;
using Microsoft.JSInterop;
using Supabase.Gotrue;
using Supabase.Gotrue.Interfaces;

namespace HealthTech.AppShell.Services;

public sealed class BlazorLocalStorageSessionPersistence : IGotrueSessionPersistence<Session>
{
    private const string StorageKey = "sb-auth";
    private readonly IJSInProcessRuntime _js;

    public BlazorLocalStorageSessionPersistence(IJSRuntime js)
    {
        _js = js as IJSInProcessRuntime
              ?? throw new NotSupportedException("Requer IJSInProcessRuntime (Blazor WASM).");
    }

    public void SaveSession(Session session)
        => _js.InvokeVoid("localStorage.setItem", StorageKey, JsonSerializer.Serialize(session));

    public Session? LoadSession()
    {
        var json = _js.Invoke<string?>("localStorage.getItem", StorageKey);
        return string.IsNullOrWhiteSpace(json) ? null : JsonSerializer.Deserialize<Session>(json);
    }

    public void DestroySession()
        => _js.InvokeVoid("localStorage.removeItem", StorageKey);
}
