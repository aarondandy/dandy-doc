﻿using DuckyDocs.CodeDoc;
using DuckyDocs.CRef;
using RazorEngine;
using RazorEngine.Templating;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckyDocs.SiteBuilder
{
    public class StaticApiPageGenerator
    {
        public static string CreateSlugName(string cRef)
        {
            var slug = cRef;
            if (slug.Length >= 2 && Char.IsLetter(slug[0]) && slug[1] == ':')
            {
                var prefixLeffter = slug[0];
                slug = String.Concat(slug.Substring(2), '-', prefixLeffter);
            }

            var slugChars = slug.ToCharArray();
            for (int i = 0; i < slugChars.Length; ++i)
            {
                var c = slugChars[i];
                if (c == '#' || c == ':')
                {
                    slugChars[i] = '-';
                }
                else if (c == '@' || c == '`')
                {
                    slugChars[i] = '!';
                }
            }

            return new string(slugChars);
        }

        public DirectoryInfo TemplateDirectory { get; set; }

        public DirectoryInfo OutputDirectory { get; set; }

        public ICodeDocMemberRepository TargetRepository { get; set; }

        public ICodeDocMemberRepository SupportingRepository { get; set; }

        public IEnumerable<FileInfo> GenerateForAllTargets(DynamicViewBag viewBag = null)
        {
            viewBag = ApplyViewBag(viewBag);

            var results = new List<FileInfo>();
            foreach (var @namespace in TargetRepository.Namespaces)
            {
                ;
                foreach (var typeCref in @namespace.TypeCRefs) {
                    var typeModel = GetModelFromTarget(typeCref);
                    if (typeModel is CodeDocType)
                    {
                        results.AddRange(GenerateForAllTypeChildTargets((CodeDocType)typeModel, viewBag));
                    }
                    results.Add(Generate(typeModel, viewBag));
                }
                results.Add(Generate(@namespace, viewBag));
            }
            return results;
        }

        public FileInfo GenerateForTarget(CRefIdentifier cRef, DynamicViewBag viewBag = null)
        {
            if (cRef == null) throw new ArgumentNullException("cRef");
            Contract.EndContractBlock();

            var model = GetModelFromTarget(cRef);
            if (model == null)
                return null;

            return Generate(model, ApplyViewBag(viewBag));
        }

        public IEnumerable<FileInfo> GenerateForAllTypeChildTargets(CodeDocType typeModel, DynamicViewBag viewBag)
        {
            var results = new List<FileInfo>();


            foreach (var childType in ReaquireFromTarget(typeModel.NestedTypes ?? Enumerable.Empty<ICodeDocMember>()))
            {
                if (childType is CodeDocType)
                {
                    results.AddRange(GenerateForAllTypeChildTargets((CodeDocType)childType, viewBag));
                }
                results.Add(Generate(childType, viewBag));
            }

            foreach (var childDelegate in ReaquireFromTarget(typeModel.NestedDelegates ?? Enumerable.Empty<ICodeDocMember>()))
            {
                results.Add(Generate(childDelegate, viewBag));
            }

            foreach (var field in ReaquireFromTarget(typeModel.Fields ?? Enumerable.Empty<ICodeDocMember>()))
            {
                results.Add(Generate(field, viewBag));
            }

            foreach (var @event in ReaquireFromTarget(typeModel.Events ?? Enumerable.Empty<ICodeDocMember>()))
            {
                results.Add(Generate(@event, viewBag));
            }

            var allMethods = typeModel.Methods ?? Enumerable.Empty<ICodeDocMember>()
                .Concat(typeModel.Constructors ?? Enumerable.Empty<ICodeDocMember>())
                .Concat(typeModel.Operators ?? Enumerable.Empty<ICodeDocMember>());
            foreach (var method in ReaquireFromTarget(allMethods))
            {
                results.Add(Generate(method, viewBag));
            }

            foreach (var property in ReaquireFromTarget(typeModel.Properties ?? Enumerable.Empty<ICodeDocMember>()))
            {
                results.Add(Generate(property, viewBag));
            }

            return results;
        }

        private FileInfo Generate(ICodeDocMember model, DynamicViewBag viewBag)
        {
            if (model == null) throw new ArgumentNullException("request");
            Contract.Ensures(Contract.Result<FileInfo>() != null);

            if (model is CodeDocType)
                return GenerateType((CodeDocType)model, viewBag);
            if (model is CodeDocEvent)
                return GenerateSimple((CodeDocEvent)model, "_event", viewBag);
            if (model is CodeDocField)
                return GenerateSimple((CodeDocField)model, "_field", viewBag);
            if (model is CodeDocMethod)
                return GenerateSimple((CodeDocMethod)model, "_method", viewBag);
            if (model is CodeDocProperty)
                return GenerateSimple((CodeDocProperty)model, "_property", viewBag);
            if (model is CodeDocSimpleNamespace)
                return GenerateSimple((CodeDocSimpleNamespace)model, "_namespace", viewBag);

            throw new NotSupportedException();
        }

        private IEnumerable<ICodeDocMember> ReaquireFromTarget(IEnumerable<ICodeDocMember> members, CodeDocMemberDetailLevel detailLevel = CodeDocMemberDetailLevel.Full)
        {
            return members
                .Select(m => GetModelFromTarget(m.CRef, detailLevel))
                .Where(m => m != null);
        }

        private ICodeDocMember GetMinimumModelFromTarget(CRefIdentifier cRef)
        {
            return GetModelFromTarget(cRef, CodeDocMemberDetailLevel.Minimum);
        }

        private ICodeDocMember GetModelFromTarget(CRefIdentifier cRef, CodeDocMemberDetailLevel detailLevel = CodeDocMemberDetailLevel.Full)
        {
            if (cRef == null) throw new ArgumentNullException("cRef");
            Contract.EndContractBlock();

            if (TargetRepository == null)
            {
                return null;
            }

            var searchRepositories = SupportingRepository == null
                ? new[] { TargetRepository }
                : new[] { TargetRepository, SupportingRepository };

            return new CodeDocRepositorySearchContext(searchRepositories, detailLevel)
                .CloneWithOneUnvisited(TargetRepository)
                .Search(cRef);
        }

        private ICodeDocMember GetModelFromSupporting(CRefIdentifier cRef, CodeDocMemberDetailLevel detailLevel = CodeDocMemberDetailLevel.Full)
        {
            if (cRef == null) throw new ArgumentNullException("cRef");
            Contract.EndContractBlock();

            if (SupportingRepository == null)
            {
                return null;
            }

            return new CodeDocRepositorySearchContext(new[] { SupportingRepository }, detailLevel)
                .Search(cRef);
        }

        private ICodeDocMember GetModelFromAny(CRefIdentifier cRef, CodeDocMemberDetailLevel detailLevel = CodeDocMemberDetailLevel.Full)
        {
            if (cRef == null) throw new ArgumentNullException("cRef");
            Contract.EndContractBlock();

            var searchRepositories = new[] { TargetRepository, SupportingRepository }
                .Where(x => x != null);

            return new CodeDocRepositorySearchContext(searchRepositories, detailLevel)
                .Search(cRef);
        }

        private DynamicViewBag ApplyViewBag(DynamicViewBag viewBag)
        {
            if (viewBag == null)
            {
                viewBag = new DynamicViewBag();
            }

            dynamic dynamicViewBag = viewBag;
            if (dynamicViewBag.GetTargetPreviewModel == null)
            {
                dynamicViewBag.GetTargetPreviewModel = (Func<CRefIdentifier, ICodeDocMember>)GetMinimumModelFromTarget;
            }

            return viewBag;
        }

        private FileInfo GenerateType(CodeDocType model, DynamicViewBag viewBag)
        {
            Contract.Requires(model != null);
            Contract.Ensures(Contract.Result<FileInfo>() != null);
            if (model is CodeDocDelegate)
            {
                return GenerateSimple((CodeDocDelegate)model, "_delegate", viewBag);
            }

            return GenerateSimple(model, "_type", viewBag);
        }

        private FileInfo GenerateSimple<TModel>(TModel model, string template, DynamicViewBag viewBag) where TModel : CodeDocSimpleMember
        {
            Contract.Requires(model != null);
            Contract.Ensures(Contract.Result<FileInfo>() != null);
            var result = ExecuteTemplate(template, model, viewBag);
            var filePath = CreateFilePath(model);
            File.WriteAllText(filePath.FullName, result);
            return filePath;
        }

        private FileInfo CreateFilePath(CodeDocSimpleMember model)
        {
            Contract.Requires(model != null);
            Contract.Ensures(Contract.Result<FileInfo>() != null);
            var fileName = CreateSlugName(model.CRefText) + ".html";
            return new FileInfo(Path.Combine(OutputDirectory.FullName, fileName));
        }

        private string ExecuteTemplate<TModel>(string templateName, TModel model, DynamicViewBag viewBag)
        {
            var cacheKey = templateName + typeof(TModel).Name;
            var resolved = Razor.Resolve<TModel>(cacheKey, model);
            if (resolved == null)
            {
                var templatePath = Path.Combine(TemplateDirectory.FullName, Path.ChangeExtension(templateName, "cshtml"));
                var templateContnet = File.ReadAllText(templatePath);
                Razor.Compile<TModel>(templateContnet, cacheKey);
            }

            return Razor.Run<TModel>(cacheKey, model, viewBag);
        }
        
    }
}
