namespace MStopwatch.FSharp

[<SealedAttribute>]
type Constants = 
    static member TimeSpanFormat = @"hh\:mm\:ss\""fff"
    static member TimeSpanFormatNoMillsecond = @"hh\:mm\:ss\"""
    static member DateTimeFormat = "yyyy/MM/dd HH:mm:ss"
    private new() = Constants()
