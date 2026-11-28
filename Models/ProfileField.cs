namespace IskolarTunes.Models;

// One bit per profile field. A bit that is ON means the field is PUBLIC.
// Email, phone and security data are deliberately NOT in this list: they can never be public.
[Flags]
public enum ProfileField
{
    None = 0,
    FullName = 1,
    College = 2,
    Program = 4,
    Birthdate = 8,
    Bio = 16,
    Location = 32,
    Website = 64,
    Pronouns = 128
}