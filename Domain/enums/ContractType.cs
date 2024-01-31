using System.ComponentModel;

public enum ContractType
{
    [Description ("Umowa zlecenie")]
    UZ,
    [Description("Umowa o pracę")]
    UoP,
    [Description("Kontrakt B2B")]
    B2B,
    [Description("Inny typ umowy")]
    Other
}