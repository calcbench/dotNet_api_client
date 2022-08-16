using System.Runtime.Serialization;

namespace Calcbench
{
    public enum FilingType
    {
        BusinessWirePR_replaced = -1,
        BusinessWirePR_filedAfterAn8K = -2,
        proxy = 1,
        annualQuarterlyReport = 2,
        eightk_earningsPressRelease = 3,
        eightk_guidanceUpdate = 4,
        eightk_conferenceCallTranscript = 5,
        eightk_presentationSlides = 6,
        eightk_monthlyOperatingMetrics = 7,
        eightk_earningsPressRelease_preliminary = 8,
        eightk_earningsPressRelease_correction = 9,
        eightk_other = 10, //  ' 2.02 OTHER 
        commentLetter = 11, // ' from sec  UPLOAD
        commentLetterResponse = 12,//  ' from company   CORRESP

        form_3 = 13,
        form_4 = 14,
        form_5 = 15,

        eightk_nonfinancial = 20,// non 2.02's
        NT10KorQ = 25,
        S = 26,
        Four24B = 27,
    }
}
