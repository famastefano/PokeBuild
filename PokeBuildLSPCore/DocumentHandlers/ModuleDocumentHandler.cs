using MediatR;

using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities;

using PokeBuildLSPCore.Models;

using System.Collections.Concurrent;
using System.Threading;

namespace PokeBuildLSPCore.DocumentHandlers;
public class ModuleDocumentHandler : ITextDocumentSyncHandler
{
    private TextDocumentSelector Selector =>
        new([new TextDocumentFilter()
        {
              Language = "csharp",
              Pattern = "**/*.Build.cs"
        }]);

    private void Execute(TextView view, CancellationToken cancellationToken, Action<TextView> action)
    {
        try
        {
            while (!view.RWLock.TryEnterWriteLock(100))
                if (cancellationToken.IsCancellationRequested)
                    return;
            action(view);
        }
        finally
        {
            if (view.RWLock.IsWriteLockHeld)
                view.RWLock.ExitWriteLock();
        }
    }

    public TextDocumentChangeRegistrationOptions GetRegistrationOptions(TextSynchronizationCapability capability, ClientCapabilities clientCapabilities)
      => new()
      {
          DocumentSelector = Selector,
          SyncKind = TextDocumentSyncKind.Incremental,
      };

    public TextDocumentAttributes GetTextDocumentAttributes(DocumentUri uri)
        => new(uri, "csharp");

    public Task<Unit> Handle(DidChangeTextDocumentParams request, CancellationToken cancellationToken)
    {
        var view = Globals.Documents.GetOrAdd(request.TextDocument.Uri, (_) => new(8192));
        Execute(view, cancellationToken, view =>
        {
            foreach (var change in request.ContentChanges)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;
                view.UpdateDocument(change.Text, change.Range);
            }
        });
        return Unit.Task;
    }

    public Task<Unit> Handle(DidOpenTextDocumentParams request, CancellationToken cancellationToken)
    {
        var view = Globals.Documents.GetOrAdd(request.TextDocument.Uri, (_) => new(8192));
        Execute(view, cancellationToken, view =>
        {
            if (!cancellationToken.IsCancellationRequested)
                view.ReplaceDocument(request.TextDocument.Text);
        });
        return Unit.Task;
    }

    public Task<Unit> Handle(DidCloseTextDocumentParams request, CancellationToken cancellationToken)
    {
        if (Globals.Documents.ContainsKey(request.TextDocument.Uri))
            Globals.Documents.Remove(request.TextDocument.Uri, out var _);
        return Unit.Task;
    }

    public Task<Unit> Handle(DidSaveTextDocumentParams request, CancellationToken cancellationToken)
    => Unit.Task;

    TextDocumentOpenRegistrationOptions IRegistration<TextDocumentOpenRegistrationOptions, TextSynchronizationCapability>.GetRegistrationOptions(TextSynchronizationCapability capability, ClientCapabilities clientCapabilities)
    => new() { DocumentSelector = Selector };

    TextDocumentCloseRegistrationOptions IRegistration<TextDocumentCloseRegistrationOptions, TextSynchronizationCapability>.GetRegistrationOptions(TextSynchronizationCapability capability, ClientCapabilities clientCapabilities)
    => new() { DocumentSelector = Selector };

    TextDocumentSaveRegistrationOptions IRegistration<TextDocumentSaveRegistrationOptions, TextSynchronizationCapability>.GetRegistrationOptions(TextSynchronizationCapability capability, ClientCapabilities clientCapabilities)
    => new() { IncludeText = false, DocumentSelector = Selector };
}