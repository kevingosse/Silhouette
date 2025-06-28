using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dnlib.DotNet;
using dnlib.DotNet.MD;
using dnlib.IO;
using dnlib.DotNet.Writer;

namespace Silhouette.IL;
public class IlRewriter
{
    private readonly ICorProfilerInfo2 CorProfilerInfo;

    public IlRewriter(ICorProfilerInfo2 corProfilerInfo)
    {
        CorProfilerInfo = corProfilerInfo;
    }

    public unsafe bool Import(IntPtr method, ModuleId module, MdMethodDef methodDefToken)
    {
        try
        {
            using var metadataImport = CorProfilerInfo
                .GetModuleMetaDataImport(module, CorOpenFlags.ofRead | CorOpenFlags.ofWrite)
                .ThrowIfFailed()
                .Wrap();

            var dataStream = DataStreamFactory.Create((byte*)method);
            var dataReader = new DataReader(dataStream, 0, uint.MaxValue);

            var resolver = new InstructionOperandResolver(metadataImport);

            var parameters = new List<Parameter>();

            // IInstructionOperandResolver
            var bodyReader = new MethodBodyReader(resolver, dataReader, parameters);

            if (!bodyReader.Read())
            {
                return false;
            }

            var body = bodyReader.CreateCilBody();

            Console.WriteLine("Dumping body");

            foreach (var instruction in body.Instructions)
            {
                Console.WriteLine(instruction);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error importing method: {ex.Message}");
            return false;
        }

        return true;
    }
}

public class TokenProvider : ITokenProvider
{
    public void Error(string message)
    {
        Console.WriteLine($"Error({message})");
        throw new NotImplementedException();
    }

    public MDToken GetToken(object o)
    {
        Console.WriteLine($"GetToken({o})");
        throw new NotImplementedException();
    }

    public MDToken GetToken(IList<TypeSig> locals, uint origToken)
    {
        Console.WriteLine($"GetToken({locals}, {origToken})");
        throw new NotImplementedException();
    }
}

public class InstructionOperandResolver : IInstructionOperandResolver, IDisposable
{
    private readonly ComPtr<IMetaDataImport> _metadataImport;

    public InstructionOperandResolver(ComPtr<IMetaDataImport> metadataImport)
    {
        _metadataImport = metadataImport.Copy();
    }

    public IMDTokenProvider ResolveToken(uint token, GenericParamContext gpContext)
    {
        Console.WriteLine($"ResolveToken({token:x2}, {gpContext.Type}, {gpContext.Method})");

        if (token == 0)
        {
            return null;
        }

        //switch (MDToken.ToTable(token))
        //{
        //    case Table.Module:
        //        return new ModuleDefUser(_metadataImport.Value.GetModule(new((int)token)).ThrowIfFailed());
        //    case Table.TypeDef:
        //        return new TypeDefUser(_metadataImport.Value.GetTypeDef(new((int)token)).ThrowIfFailed());
        //    case Table.MethodDef:
        //        return new MethodDefUser(_metadataImport.Value.GetMethodDef(new((int)token)).ThrowIfFailed());
        //    case Table.Field:
        //        return new FieldDefUser(_metadataImport.Value.GetField(new((int)token)).ThrowIfFailed());
        //    case Table.Param:
        //        return new ParamDefUser(_metadataImport.Value.GetParam(new((int)token)).ThrowIfFailed());
        //    case Table.MemberRef:
        //        return new MemberRefUser(_metadataImport.Value.GetMemberRef(new((int)token)).ThrowIfFailed());
        //    case Table.String:
        //        return _metadataImport.Value.GetString(new((int)token)).ThrowIfFailed();
        //    case Table.UserString:
        //        return ReadUserString(token);
        //    default:
        //        throw new NotSupportedException($"Token type {MDToken.ToTable(token)} is not supported.");
        //}

        return MDToken.ToTable(token) switch
        {
            //Table.Module => ResolveModule(rid),
            //Table.TypeRef => ResolveTypeRef(rid),
            //Table.TypeDef => ResolveTypeDef(rid),
            //Table.Field => ResolveField(rid),
            //Table.Method => ResolveMethod(rid),
            //Table.Param => ResolveParam(rid),
            //Table.InterfaceImpl => ResolveInterfaceImpl(rid, gpContext),
            Table.MemberRef => ResolveMemberRef(token, gpContext),
            //Table.Constant => ResolveConstant(rid),
            //Table.DeclSecurity => ResolveDeclSecurity(rid),
            //Table.ClassLayout => ResolveClassLayout(rid),
            //Table.StandAloneSig => ResolveStandAloneSig(rid, gpContext),
            //Table.Event => ResolveEvent(rid),
            //Table.Property => ResolveProperty(rid),
            //Table.ModuleRef => ResolveModuleRef(rid),
            //Table.TypeSpec => ResolveTypeSpec(rid, gpContext),
            //Table.ImplMap => ResolveImplMap(rid),
            //Table.Assembly => ResolveAssembly(rid),
            //Table.AssemblyRef => ResolveAssemblyRef(rid),
            //Table.File => ResolveFile(rid),
            //Table.ExportedType => ResolveExportedType(rid),
            //Table.ManifestResource => ResolveManifestResource(rid),
            //Table.GenericParam => ResolveGenericParam(rid),
            //Table.MethodSpec => ResolveMethodSpec(rid, gpContext),
            //Table.GenericParamConstraint => ResolveGenericParamConstraint(rid, gpContext),
            _ => null,
        };


        throw new NotImplementedException();
    }

    private unsafe MemberRef ResolveMemberRef(uint token, GenericParamContext gpContext)
    {
        Console.WriteLine($"ResolveMemberRef({token:x2}, {gpContext.Type}, {gpContext.Method})");
        var memberRef = _metadataImport.Value.GetMemberRefProps(new((int)token)).ThrowIfFailed();

        var moduleDef = new ModuleDefUser();

        var props = _metadataImport.Value.GetMemberRefProps(new MdMemberRef((int)token)).ThrowIfFailed();

        Console.WriteLine($"Parent: {props.Token.Value.ToString("x2")}");

        IMemberRefParent parent = null;

        if (MDToken.ToTable(props.Token.Value) == Table.TypeRef)
        {
            Console.WriteLine("Parent is TypeRef.");
            parent = ResolveTypeRef((uint)props.Token.Value, gpContext);
        }
        else
        {
            Console.WriteLine(MDToken.ToTable(props.Token.Value));
        }

        var dataStream = DataStreamFactory.Create((byte*)props.Signature.Ptr);
        var dataReader = new DataReader(dataStream, 0, (uint)props.Signature.Length);

        var sig = SignatureReader.ReadSig(new SignatureReaderHelper(), new CorLibTypes(), dataReader);

        return new MyMemberRef(props.Name, MDToken.ToRID(token), sig, parent);
    }

    private IMemberRefParent ResolveTypeRef(uint tokenValue, GenericParamContext gpContext)
    {
        Console.WriteLine($"ResolveTypeRef({tokenValue:x2}, {gpContext.Type}, {gpContext.Method})");
        var typeRefProps = _metadataImport.Value.GetTypeRefProps(new((int)tokenValue)).ThrowIfFailed();
        return new TypeRefUser(new ModuleDefUser(new("TypeRef-ModuleDefUser")), new(typeRefProps.TypeName));
    }

    //private IMemberRefParent ResolveFieldLayout(uint tokenValue, GenericParamContext gpContext)
    //{
    //    Console.WriteLine($"ResolveFieldLayout({tokenValue:x2}, {gpContext.Type}, {gpContext.Method})");
    //    var props = _metadataImport.Value.GetFieldProps(new((int)tokenValue)).ThrowIfFailed();

    //    var @class = props.Class;

    //    var typeProps = _metadataImport.Value.GetTypeDefProps(@class).ThrowIfFailed();

    //    return new TypeDefUser(typeProps.TypeName);
    //}

    private class MyMemberRef : MemberRef
    {
        public MyMemberRef(string name, uint rid, CallingConventionSig sig, IMemberRefParent parent)
        {
            this.name = name;
            this.rid = rid;
            this.module = new ModuleDefUser(new("MyModule"));
            this.@class = parent;
            //var sig = MethodSig.CreateStatic(new ClassSig(new TypeDefUser(new("TypeDefArg"))));
            //SignatureReader.ReadSig()
            this.signature = sig;
        }

        public override string ToString()
        {
            return "MyMemberRef";
        }
    }

    public string ReadUserString(uint token)
    {
        Console.WriteLine($"ReadUserString({token:x2})");

        var str = _metadataImport.Value.GetUserString(new((int)token)).ThrowIfFailed();
        Console.WriteLine($"Returning {str}");
        return str;
    }

    public void Dispose()
    {
        _metadataImport.Dispose();
    }
}

public class SignatureReaderHelper : ISignatureReaderHelper
{
    public ITypeDefOrRef ResolveTypeDefOrRef(uint codedToken, GenericParamContext gpContext)
    {
        Console.WriteLine($"ISignatureReaderHelper.ResolveTypeDefOrRef({codedToken:x2}, {gpContext.Type}, {gpContext.Method})");
        throw new NotImplementedException();
    }

    public TypeSig ConvertRTInternalAddress(IntPtr address)
    {
        Console.WriteLine($"ISignatureReaderHelper.ConvertRTInternalAddress({address})");
        throw new NotImplementedException();
    }
}

public class CorLibTypes : ICorLibTypes
{
    public TypeRef GetTypeRef(string @namespace, string name)
    {
        Console.WriteLine($"GetTypeRef({@namespace}, {name})");
        throw new NotImplementedException();
    }

    public CorLibTypeSig Void { get; }
    public CorLibTypeSig Boolean { get; }
    public CorLibTypeSig Char { get; }
    public CorLibTypeSig SByte { get; }
    public CorLibTypeSig Byte { get; }
    public CorLibTypeSig Int16 { get; }
    public CorLibTypeSig UInt16 { get; }
    public CorLibTypeSig Int32 { get; }
    public CorLibTypeSig UInt32 { get; }
    public CorLibTypeSig Int64 { get; }
    public CorLibTypeSig UInt64 { get; }
    public CorLibTypeSig Single { get; }
    public CorLibTypeSig Double { get; }
    public CorLibTypeSig String { get
        {
            Console.WriteLine("Returning String type signature.");
            return default;
        }
    }
    public CorLibTypeSig TypedReference { get; }
    public CorLibTypeSig IntPtr { get; }
    public CorLibTypeSig UIntPtr { get; }
    public CorLibTypeSig Object { get; }
    public AssemblyRef AssemblyRef { get; }
}