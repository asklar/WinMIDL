#include <winmd_reader.h>

#include <cstdio>
#include <cstdint>
#include <iomanip>
#include <iostream>
#include <sstream>
#include <string>
#include <variant>

using namespace winmd::reader;

// --- Type signature formatting ---

static std::string format_type_sig(TypeSig const& sig);

static std::string format_coded_type(coded_index<TypeDefOrRef> const& idx)
{
    if (!idx) return "<null>";
    auto [ns, name] = get_type_namespace_and_name(idx);
    if (ns.empty()) return std::string(name);
    return std::string(ns) + "." + std::string(name);
}

static std::string format_generic_inst(GenericTypeInstSig const& g)
{
    std::string s = format_coded_type(g.GenericType()) + "<";
    auto [first, last] = g.GenericArgs();
    bool comma = false;
    for (auto it = first; it != last; ++it)
    {
        if (comma) s += ", ";
        s += format_type_sig(*it);
        comma = true;
    }
    s += ">";
    return s;
}

static std::string element_type_name(ElementType e)
{
    switch (e)
    {
    case ElementType::Void:    return "Void";
    case ElementType::Boolean: return "Boolean";
    case ElementType::Char:    return "Char";
    case ElementType::I1:      return "Int8";
    case ElementType::U1:      return "UInt8";
    case ElementType::I2:      return "Int16";
    case ElementType::U2:      return "UInt16";
    case ElementType::I4:      return "Int32";
    case ElementType::U4:      return "UInt32";
    case ElementType::I8:      return "Int64";
    case ElementType::U8:      return "UInt64";
    case ElementType::R4:      return "Single";
    case ElementType::R8:      return "Double";
    case ElementType::String:  return "String";
    case ElementType::Object:  return "Object";
    case ElementType::I:       return "IntPtr";
    case ElementType::U:       return "UIntPtr";
    default:
    {
        char buf[32];
        snprintf(buf, sizeof(buf), "ElementType(0x%02X)", static_cast<unsigned>(e));
        return buf;
    }
    }
}

static std::string format_type_sig(TypeSig const& sig)
{
    std::string result;

    auto const& v = sig.Type();
    if (auto et = std::get_if<ElementType>(&v))
    {
        result = element_type_name(*et);
    }
    else if (auto ci = std::get_if<coded_index<TypeDefOrRef>>(&v))
    {
        result = format_coded_type(*ci);
    }
    else if (auto gi = std::get_if<GenericTypeInstSig>(&v))
    {
        result = format_generic_inst(*gi);
    }
    else if (auto ti = std::get_if<GenericTypeIndex>(&v))
    {
        result = "!" + std::to_string(ti->index);
    }
    else if (auto mi = std::get_if<GenericMethodTypeIndex>(&v))
    {
        result = "!!" + std::to_string(mi->index);
    }
    else
    {
        result = "???";
    }

    if (sig.is_szarray()) result += "[]";
    for (int i = 0; i < sig.ptr_count(); ++i) result += "*";

    return result;
}

// --- Constant formatting ---

static std::string format_constant(Constant::constant_type const& val)
{
    struct Visitor
    {
        std::string operator()(bool v) const { return v ? "true" : "false"; }
        std::string operator()(char16_t v) const
        {
            char buf[32]; snprintf(buf, sizeof(buf), "'\\u%04X'", static_cast<unsigned>(v)); return buf;
        }
        std::string operator()(int8_t v) const { return std::to_string(v); }
        std::string operator()(uint8_t v) const { return std::to_string(v); }
        std::string operator()(int16_t v) const { return std::to_string(v); }
        std::string operator()(uint16_t v) const { return std::to_string(v); }
        std::string operator()(int32_t v) const { return std::to_string(v); }
        std::string operator()(uint32_t v) const { return std::to_string(v); }
        std::string operator()(int64_t v) const { return std::to_string(v); }
        std::string operator()(uint64_t v) const { return std::to_string(v); }
        std::string operator()(float v) const
        {
            char buf[64]; snprintf(buf, sizeof(buf), "%g", v); return buf;
        }
        std::string operator()(double v) const
        {
            char buf[64]; snprintf(buf, sizeof(buf), "%g", v); return buf;
        }
        std::string operator()(std::u16string_view) const { return "<string>"; }
        std::string operator()(std::nullptr_t) const { return "null"; }
    };
    return std::visit(Visitor{}, val);
}

static std::string constant_type_name(ConstantType t)
{
    switch (t)
    {
    case ConstantType::Boolean: return "Boolean";
    case ConstantType::Char:    return "Char";
    case ConstantType::Int8:    return "Int8";
    case ConstantType::UInt8:   return "UInt8";
    case ConstantType::Int16:   return "Int16";
    case ConstantType::UInt16:  return "UInt16";
    case ConstantType::Int32:   return "Int32";
    case ConstantType::UInt32:  return "UInt32";
    case ConstantType::Int64:   return "Int64";
    case ConstantType::UInt64:  return "UInt64";
    case ConstantType::Float32: return "Single";
    case ConstantType::Float64: return "Double";
    case ConstantType::String:  return "String";
    case ConstantType::Class:   return "Class";
    default:
    {
        char buf[32]; snprintf(buf, sizeof(buf), "0x%02X", static_cast<unsigned>(t)); return buf;
    }
    }
}

// --- Custom attribute arg formatting ---

static std::string format_elem_sig(ElemSig::value_type const& val)
{
    struct V
    {
        std::string operator()(bool v) const { return std::string("(Boolean) ") + (v ? "true" : "false"); }
        std::string operator()(char16_t v) const
        {
            char buf[48]; snprintf(buf, sizeof(buf), "(Char) '\\u%04X'", static_cast<unsigned>(v)); return buf;
        }
        std::string operator()(int8_t v) const { return "(Int8) " + std::to_string(v); }
        std::string operator()(uint8_t v) const { return "(UInt8) " + std::to_string(v); }
        std::string operator()(int16_t v) const { return "(Int16) " + std::to_string(v); }
        std::string operator()(uint16_t v) const { return "(UInt16) " + std::to_string(v); }
        std::string operator()(int32_t v) const { return "(Int32) " + std::to_string(v); }
        std::string operator()(uint32_t v) const { return "(UInt32) " + std::to_string(v); }
        std::string operator()(int64_t v) const { return "(Int64) " + std::to_string(v); }
        std::string operator()(uint64_t v) const { return "(UInt64) " + std::to_string(v); }
        std::string operator()(float v) const
        {
            char buf[64]; snprintf(buf, sizeof(buf), "(Single) %g", v); return buf;
        }
        std::string operator()(double v) const
        {
            char buf[64]; snprintf(buf, sizeof(buf), "(Double) %g", v); return buf;
        }
        std::string operator()(std::string_view v) const
        {
            return "(String) \"" + std::string(v) + "\"";
        }
        std::string operator()(ElemSig::SystemType const& v) const
        {
            return "(Type) " + std::string(v.name);
        }
        std::string operator()(ElemSig::EnumValue const& v) const
        {
            // Print the enum type name + underlying value
            std::string enum_name;
            auto ns = v.type.m_typedef.TypeNamespace();
            auto n = v.type.m_typedef.TypeName();
            if (!ns.empty()) { enum_name = std::string(ns) + "."; }
            enum_name += n;

            std::string val_str = std::visit([](auto x) -> std::string { return std::to_string(x); }, v.value);
            return "(" + enum_name + ") " + val_str;
        }
    };
    return std::visit(V{}, val);
}

// Helper to access protected get_blob on CustomAttribute rows
struct ca_blob_accessor : winmd::reader::CustomAttribute
{
    winmd::reader::byte_view raw_blob() const { return get_blob(2); }
};

static void dump_custom_attributes(auto const& owner, std::string const& indent)
{
    int ca_idx = 0;
    for (auto ca : owner.CustomAttribute())
    {
        auto [ca_ns, ca_name] = ca.TypeNamespaceAndName();
        std::string full_name;
        if (!ca_ns.empty()) full_name = std::string(ca_ns) + ".";
        full_name += ca_name;
        std::cout << indent << "CustomAttribute[" << ca_idx << "]: " << full_name << "\n";

        try
        {
            auto sig = ca.Value();
            auto const& fixed = sig.FixedArgs();
            for (size_t i = 0; i < fixed.size(); ++i)
            {
                auto const& arg = fixed[i];
                if (auto elem = std::get_if<ElemSig>(&arg.value))
                {
                    std::cout << indent << "  Arg[" << i << "]: " << format_elem_sig(elem->value) << "\n";
                }
                else if (auto arr = std::get_if<std::vector<ElemSig>>(&arg.value))
                {
                    std::cout << indent << "  Arg[" << i << "]: {array, " << arr->size() << " elements}\n";
                    for (size_t j = 0; j < arr->size(); ++j)
                    {
                        std::cout << indent << "    [" << j << "]: " << format_elem_sig((*arr)[j].value) << "\n";
                    }
                }
            }
            auto const& named = sig.NamedArgs();
            for (size_t i = 0; i < named.size(); ++i)
            {
                auto const& na = named[i];
                std::string val_str;
                if (auto elem = std::get_if<ElemSig>(&na.value.value))
                {
                    val_str = format_elem_sig(elem->value);
                }
                else
                {
                    val_str = "{array}";
                }
                std::cout << indent << "  Named[" << na.name << "]: " << val_str << "\n";
            }
        }
        catch (...)
        {
            // The high-level Value() decoder fails when constructor parameters reference
            // enum types in external assemblies (e.g. ThreadingModel, MarshalingType in
            // Windows.Foundation.Metadata). Fall back to raw blob decoding.
            try
            {
                auto const& accessor = static_cast<ca_blob_accessor const&>(ca);
                auto blob = accessor.raw_blob();
                auto sz = blob.size();
                if (sz >= 6 && blob.as<uint16_t>(0) == 0x0001)
                {
                    // Single Int32/enum parameter: prolog(2) + value(4) + named_count(2) = 8
                    if (sz == 8)
                    {
                        int32_t val = blob.as<int32_t>(2);
                        std::cout << indent << "  Arg[0]: (Int32) " << val << "\n";
                    }
                    // Two-parameter (String + Int32): ActivatableAttribute, StaticAttribute, etc.
                    else if (sz > 8)
                    {
                        std::cout << indent << "  <raw blob, " << sz << " bytes>\n";
                    }
                    else
                    {
                        std::cout << indent << "  <failed to decode attribute value>\n";
                    }
                }
                else
                {
                    std::cout << indent << "  <failed to decode attribute value>\n";
                }
            }
            catch (...)
            {
                std::cout << indent << "  <failed to decode attribute value>\n";
            }
        }

        ++ca_idx;
    }
}

// --- Flag descriptions ---

static std::string type_flags_desc(uint32_t f)
{
    std::string s;
    auto add = [&](char const* n) { if (!s.empty()) s += " | "; s += n; };

    uint32_t vis = f & 0x7;
    switch (vis)
    {
    case 0x00: add("NotPublic"); break;
    case 0x01: add("Public"); break;
    case 0x02: add("NestedPublic"); break;
    case 0x03: add("NestedPrivate"); break;
    case 0x04: add("NestedFamily"); break;
    case 0x05: add("NestedAssembly"); break;
    case 0x06: add("NestedFamANDAssem"); break;
    case 0x07: add("NestedFamORAssem"); break;
    }
    uint32_t layout = f & 0x18;
    if (layout == 0x08) add("SequentialLayout");
    else if (layout == 0x10) add("ExplicitLayout");

    if (f & 0x20) add("Interface");
    if (f & 0x80) add("Abstract");
    if (f & 0x100) add("Sealed");
    if (f & 0x400) add("SpecialName");
    if (f & 0x800) add("RTSpecialName");
    if (f & 0x1000) add("Import");
    if (f & 0x2000) add("Serializable");
    if (f & 0x4000) add("WindowsRuntime");

    uint32_t str_format = f & 0x30000;
    if (str_format == 0x10000) add("UnicodeClass");
    else if (str_format == 0x20000) add("AutoClass");

    if (f & 0x40000) add("HasSecurity");
    if (f & 0x100000) add("BeforeFieldInit");

    return s;
}

static std::string method_flags_desc(uint16_t f)
{
    std::string s;
    auto add = [&](char const* n) { if (!s.empty()) s += " | "; s += n; };

    uint16_t access = f & 0x7;
    switch (access)
    {
    case 0x01: add("Private"); break;
    case 0x02: add("FamANDAssem"); break;
    case 0x03: add("Assembly"); break;
    case 0x04: add("Family"); break;
    case 0x05: add("FamORAssem"); break;
    case 0x06: add("Public"); break;
    }

    if (f & 0x0010) add("Static");
    if (f & 0x0020) add("Final");
    if (f & 0x0040) add("Virtual");
    if (f & 0x0080) add("HideBySig");
    if (f & 0x0100) add("NewSlot");
    if (f & 0x0200) add("Strict");
    if (f & 0x0400) add("Abstract");
    if (f & 0x0800) add("SpecialName");
    if (f & 0x1000) add("PInvokeImpl");
    if (f & 0x2000) add("UnmanagedExport");
    if (f & 0x4000) add("RTSpecialName");
    if (f & 0x8000) add("HasSecurity");

    return s;
}

static std::string field_flags_desc(uint16_t f)
{
    std::string s;
    auto add = [&](char const* n) { if (!s.empty()) s += " | "; s += n; };

    uint16_t access = f & 0x7;
    switch (access)
    {
    case 0x01: add("Private"); break;
    case 0x02: add("FamANDAssem"); break;
    case 0x03: add("Assembly"); break;
    case 0x04: add("Family"); break;
    case 0x05: add("FamORAssem"); break;
    case 0x06: add("Public"); break;
    }

    if (f & 0x0010) add("Static");
    if (f & 0x0020) add("InitOnly");
    if (f & 0x0040) add("Literal");
    if (f & 0x0080) add("NotSerialized");
    if (f & 0x0100) add("HasFieldRVA");
    if (f & 0x0200) add("SpecialName");
    if (f & 0x0400) add("RTSpecialName");
    if (f & 0x1000) add("HasFieldMarshal");
    if (f & 0x8000) add("HasDefault");

    return s;
}

static std::string param_flags_desc(uint16_t f)
{
    std::string s;
    auto add = [&](char const* n) { if (!s.empty()) s += " | "; s += n; };
    if (f & 0x0001) add("In");
    if (f & 0x0002) add("Out");
    if (f & 0x0010) add("Optional");
    if (f & 0x1000) add("HasDefault");
    if (f & 0x2000) add("HasFieldMarshal");
    return s;
}

// --- Type kind heuristic ---

static std::string infer_type_kind(TypeDef const& td)
{
    auto flags = td.Flags();
    auto extends = td.Extends();

    if (flags.Semantics() == TypeSemantics::Interface)
        return "Interface";

    if (extends)
    {
        auto [bns, bname] = get_type_namespace_and_name(extends);
        if (bns == "System" && bname == "Enum")
            return "Enum";
        if (bns == "System" && bname == "ValueType")
            return "Struct";
        if (bns == "System" && bname == "MulticastDelegate")
            return "Delegate";
        if (bns == "System" && bname == "Attribute")
            return "Attribute";
    }

    if (flags.WindowsRuntime() && flags.Sealed())
        return "RuntimeClass";
    if (flags.WindowsRuntime() && !flags.Sealed())
        return "RuntimeClass (unsealed)";

    return "Class";
}

// --- Dump routines ---

static void dump_methods(TypeDef const& td)
{
    int idx = 0;
    for (auto method : td.MethodList())
    {
        auto mflags = method.Flags();
        uint16_t raw = mflags.value;
        char hex[16];
        snprintf(hex, sizeof(hex), "0x%04X", raw);

        std::cout << "  Method[" << idx << "]: " << method.Name() << "\n";
        std::cout << "    Flags: " << hex;
        auto desc = method_flags_desc(raw);
        if (!desc.empty()) std::cout << " (" << desc << ")";
        std::cout << "\n";

        // Impl flags
        auto impl = method.ImplFlags();
        if (impl.value)
        {
            char ibuf[16]; snprintf(ibuf, sizeof(ibuf), "0x%04X", impl.value);
            std::cout << "    ImplFlags: " << ibuf << "\n";
        }

        // Signature
        try
        {
            auto sig = method.Signature();
            auto const& ret = sig.ReturnType();
            if (ret) std::cout << "    ReturnType: " << format_type_sig(ret.Type()) << "\n";
            else std::cout << "    ReturnType: Void\n";

            // Param table entries (for names & flags)
            auto params = method.ParamList();
            auto [sig_begin, sig_end] = sig.Params();
            int pidx = 0;
            for (auto param_row = params.first; param_row != params.second; ++param_row)
            {
                auto seq = param_row.Sequence();
                if (seq == 0) continue; // return type param entry

                auto pflags = param_row.Flags();
                uint16_t praw = pflags.value;
                char phex[16]; snprintf(phex, sizeof(phex), "0x%04X", praw);

                // Get the type from the signature
                std::string type_str = "???";
                auto sig_offset = seq - 1;
                if (sig_begin + sig_offset < sig_end)
                {
                    auto const& psig = *(sig_begin + sig_offset);
                    type_str = format_type_sig(psig.Type());
                    if (psig.ByRef()) type_str += "&";
                }

                std::cout << "    Param[" << pidx << "]: " << param_row.Name()
                          << " (" << type_str << ")";
                auto pdesc = param_flags_desc(praw);
                if (!pdesc.empty()) std::cout << " [" << pdesc << "]";
                std::cout << "\n";

                // Param default value
                auto pc = param_row.Constant();
                if (pc)
                {
                    std::cout << "      Default: (" << constant_type_name(pc.Type()) << ") "
                              << format_constant(pc.Value()) << "\n";
                }

                ++pidx;
            }

            if (sig.GenericParamCount() > 0)
            {
                std::cout << "    GenericParamCount: " << sig.GenericParamCount() << "\n";
            }
        }
        catch (...)
        {
            std::cout << "    <failed to decode signature>\n";
        }

        // Generic params from GenericParam table
        for (auto gp : method.GenericParam())
        {
            std::cout << "    GenericParam: " << gp.Name() << " (index=" << gp.Number() << ")\n";
        }

        dump_custom_attributes(method, "    ");
        ++idx;
    }
}

static void dump_fields(TypeDef const& td)
{
    int idx = 0;
    for (auto field : td.FieldList())
    {
        auto fflags = field.Flags();
        uint16_t raw = fflags.value;
        char hex[16]; snprintf(hex, sizeof(hex), "0x%04X", raw);

        std::cout << "  Field[" << idx << "]: " << field.Name() << "\n";
        std::cout << "    Flags: " << hex;
        auto desc = field_flags_desc(raw);
        if (!desc.empty()) std::cout << " (" << desc << ")";
        std::cout << "\n";

        try
        {
            auto sig = field.Signature();
            std::cout << "    Type: " << format_type_sig(sig.Type()) << "\n";
        }
        catch (...)
        {
            std::cout << "    <failed to decode field signature>\n";
        }

        auto fc = field.Constant();
        if (fc)
        {
            std::cout << "    Default: (" << constant_type_name(fc.Type()) << ") "
                      << format_constant(fc.Value()) << "\n";
        }

        dump_custom_attributes(field, "    ");
        ++idx;
    }
}

static void dump_properties(TypeDef const& td)
{
    int idx = 0;
    for (auto prop : td.PropertyList())
    {
        std::cout << "  Property[" << idx << "]: " << prop.Name() << "\n";

        auto pflags = prop.Flags();
        if (pflags.value)
        {
            char hex[16]; snprintf(hex, sizeof(hex), "0x%04X", pflags.value);
            std::cout << "    Flags: " << hex << "\n";
        }

        try
        {
            auto psig = prop.Type();
            std::cout << "    Type: " << format_type_sig(psig.Type()) << "\n";
        }
        catch (...)
        {
            std::cout << "    <failed to decode property signature>\n";
        }

        for (auto ms : prop.MethodSemantic())
        {
            auto sem = ms.Semantic();
            std::string kind;
            if (sem.Getter()) kind = "Getter";
            else if (sem.Setter()) kind = "Setter";
            else if (sem.Other()) kind = "Other";
            else kind = "Unknown";

            std::cout << "    " << kind << ": " << ms.Method().Name() << "\n";
        }

        auto pc = prop.Constant();
        if (pc)
        {
            std::cout << "    Default: (" << constant_type_name(pc.Type()) << ") "
                      << format_constant(pc.Value()) << "\n";
        }

        dump_custom_attributes(prop, "    ");
        ++idx;
    }
}

static void dump_events(TypeDef const& td)
{
    int idx = 0;
    for (auto evt : td.EventList())
    {
        std::cout << "  Event[" << idx << "]: " << evt.Name() << "\n";

        auto eflags = evt.EventFlags();
        if (eflags.value)
        {
            char hex[16]; snprintf(hex, sizeof(hex), "0x%04X", eflags.value);
            std::cout << "    Flags: " << hex << "\n";
        }

        auto event_type = evt.EventType();
        if (event_type)
        {
            std::cout << "    Type: " << format_coded_type(event_type) << "\n";
        }

        for (auto ms : evt.MethodSemantic())
        {
            auto sem = ms.Semantic();
            std::string kind;
            if (sem.AddOn()) kind = "AddOn";
            else if (sem.RemoveOn()) kind = "RemoveOn";
            else if (sem.Fire()) kind = "Fire";
            else if (sem.Other()) kind = "Other";
            else kind = "Unknown";

            std::cout << "    " << kind << ": " << ms.Method().Name() << "\n";
        }

        dump_custom_attributes(evt, "    ");
        ++idx;
    }
}

static void dump_interface_impls(TypeDef const& td)
{
    int idx = 0;
    for (auto ii : td.InterfaceImpl())
    {
        auto iface_type = ii.Interface();
        std::cout << "  InterfaceImpl[" << idx << "]: " << format_coded_type(iface_type) << "\n";
        dump_custom_attributes(ii, "    ");
        ++idx;
    }
}

static void dump_type(TypeDef const& td)
{
    auto ns = td.TypeNamespace();
    auto name = td.TypeName();
    std::string full_name;
    if (!ns.empty()) full_name = std::string(ns) + ".";
    full_name += name;

    auto flags = td.Flags();
    uint32_t raw_flags = flags.value;
    char hex[16]; snprintf(hex, sizeof(hex), "0x%08X", raw_flags);

    std::cout << "\n=== Type: " << full_name << " ===\n";
    std::cout << "  Kind: " << infer_type_kind(td) << "\n";
    std::cout << "  Flags: " << hex;
    auto desc = type_flags_desc(raw_flags);
    if (!desc.empty()) std::cout << " (" << desc << ")";
    std::cout << "\n";

    auto extends = td.Extends();
    if (extends)
    {
        std::cout << "  Base: " << format_coded_type(extends) << "\n";
    }

    // Generic params
    for (auto gp : td.GenericParam())
    {
        std::cout << "  GenericParam[" << gp.Number() << "]: " << gp.Name() << "\n";
    }

    dump_interface_impls(td);
    dump_methods(td);
    dump_fields(td);
    dump_properties(td);
    dump_events(td);
    dump_custom_attributes(td, "  ");
}

int main(int argc, char** argv)
{
    if (argc < 2)
    {
        std::cerr << "Usage: winmd-dump <path-to-winmd>\n";
        return 1;
    }

    char const* path = argv[1];

    try
    {
        // Use cache (not bare database) so that custom attribute decoding
        // can resolve enum types via get_cache().
        std::string file_path{path};
        winmd::reader::cache the_cache{file_path};
        auto const& db = the_cache.databases().front();

        // Assembly info
        if (db.Assembly.size() > 0)
        {
            auto assembly = db.Assembly[0];
            std::cout << "=== Assembly ===\n";
            std::cout << "  Name: " << assembly.Name() << "\n";
            auto ver = assembly.Version();
            std::cout << "  Version: " << ver.MajorVersion << "." << ver.MinorVersion
                      << "." << ver.BuildNumber << "." << ver.RevisionNumber << "\n";

            auto aflags = assembly.Flags();
            if (aflags.value)
            {
                char hex[16]; snprintf(hex, sizeof(hex), "0x%08X", aflags.value);
                std::cout << "  Flags: " << hex;
                if (aflags.WindowsRuntime()) std::cout << " (WindowsRuntime)";
                std::cout << "\n";
            }

            auto culture = assembly.Culture();
            if (!culture.empty())
            {
                std::cout << "  Culture: " << culture << "\n";
            }
        }

        // AssemblyRef table
        if (db.AssemblyRef.size() > 0)
        {
            std::cout << "\n=== AssemblyRef ===\n";
            for (uint32_t i = 0; i < db.AssemblyRef.size(); ++i)
            {
                auto aref = db.AssemblyRef[i];
                auto ver = aref.Version();
                std::cout << "  [" << i << "] " << aref.Name()
                          << " " << ver.MajorVersion << "." << ver.MinorVersion
                          << "." << ver.BuildNumber << "." << ver.RevisionNumber << "\n";
            }
        }

        // Module table
        if (db.Module.size() > 0)
        {
            auto mod = db.Module[0];
            std::cout << "\n=== Module ===\n";
            std::cout << "  Name: " << mod.Name() << "\n";
        }

        // Dump all types
        for (auto td : db.TypeDef)
        {
            dump_type(td);
        }
    }
    catch (std::exception const& e)
    {
        std::cerr << "Error: " << e.what() << "\n";
        return 1;
    }

    return 0;
}
