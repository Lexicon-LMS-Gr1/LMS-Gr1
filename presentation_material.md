# 🎓 Redovisningsmanus — Journey 4 & 5 + Kodgenomgång

> **Total tid:** ~10–12 min  
> **Språk:** Svenska  
> **Dina Journeys:**  
> - Journey 4: Lärare administrerar användare och kursdeltagare  
> - Journey 5: Lärare publicerar material till kurs, modul eller aktivitet  
> - Kodgenomgång: Dokumentuppladdning (full-stack)

---

## ⏱️ Tidsöversikt

| Del | Tid | Innehåll |
|-----|-----|----------|
| Intro | ~1 min | Presentera dig och dina ansvarsområden |
| Journey 4 | ~4 min | Användarhantering: skapa, redigera, tilldela kurs, radera |
| Journey 5 | ~4 min | Dokumentuppladdning till kurs, modul & aktivitet + radering |
| Kodgenomgång | ~3 min | Visa & förklara koden bakom filuppladdningen |
| Avslut | ~30 sek | Sammanfatta och lämna över |

---

## 🎤 Introduktion (~1 min)

> *"Hej allihopa! Jag heter [DITT NAMN] och jag har ansvarat för två centrala delar i vårt LMS:*  
> *Den första är **användarhantering** — att lärare kan skapa, redigera och ta bort användare och koppla elever till rätt kurs.*  
> *Den andra är **dokumenthantering** — att lärare kan ladda upp kursmaterial på kurs-, modul- och aktivitetsnivå.*  
> *Jag visar först båda flödena som live-demo, och sedan gör jag en kort kodgenomgång."*

---

## 🔵 Journey 4 — Lärare administrerar användare (~4 min)

### Steg 1 — Navigera till Användarhantering

**Klicka:** "Användare" i vänstermenyn.

> *"Vi befinner oss redan inloggade som lärare. Jag går till 'Användare' i sidomenyn. Här ser vi alla användare i systemet — både lärare och elever — med namn, e-post, roll och vilken kurs varje elev tillhör."*

---

### Steg 2 — Skapa ny elev

**Klicka:** Knappen **"Skapa ny användare"** (blå knappen uppe till höger).

> *"Nu skapar vi en ny elev. Jag klickar på 'Skapa ny användare'."*

**Fyll i formuläret:**

| Fält | Vad du skriver | Vad du säger |
|------|---------------|-------------|
| Förnamn | `Test` | *"Jag fyller i förnamn..."* |
| Efternamn | `Elev` | *"...efternamn..."* |
| E-post | `test.elev@lexicon.se` | *"...och en e-postadress."* |
| Lösenord | `Test123!` | *"Lösenord med minst 6 tecken."* |
| Roll | **Elev** | 👇 Se nedan |

> *"Nu väljer jag rollen 'Elev'. Observera — en **kurs-dropdown** dyker automatiskt upp. Det är ett krav att varje elev måste tillhöra exakt en kurs."*

**Välj** en kurs → **Klicka** "Skapa användare"

> *"Användaren skapades! Eleven dyker upp i listan med sin kurs."*

---

### Steg 3 — Redigera en användare

**Klicka:** Penna-ikonen på test-eleven.

> *"Vi kan redigera befintliga användare. Här kan jag ändra namn och e-post. Rollen kan inte ändras efter skapandet."*

**Ändra** efternamnet → **Klicka** "Spara ändringar"

---

### Steg 4 — Skyddsregler och radering

> *"Vi har byggt in skyddsregler — en lärare kan inte ta bort sitt eget konto, och den sista läraren kan inte raderas."*

**Klicka:** Soptunna på test-eleven → Bekräfta borttagning.

> *"Användaren raderas med bekräftelsedialog."*

---

## 🟢 Journey 5 — Lärare publicerar material (~4 min)

### Steg 5 — Navigera till en kurs

**Klicka:** "Kurser" → Välj en kurs.

> *"Nu visar jag dokumenthanteringen. Jag öppnar en kurs. Här ser vi 'Kursdokument' med en 'Ladda upp'-knapp."*

---

### Steg 6 — Ladda upp dokument till kursen

**Klicka:** "Ladda upp" (vid Kursdokument)

> *"Jag klickar 'Ladda upp' och fyller i dokumentnamn."*

| Fält | Vad du skriver |
|------|---------------|
| Namn | `Kursöversikt HT2026` |
| Beskrivning | `Schema och kursinformation` |

**Välj fil** (en PDF) → **Klicka** "Ladda upp"

> *"Systemet visar filnamnet och storleken. Vi validerar filtyp i frontend — bara PDF, DOCX, PPTX etc. Max 50 MB. Dokumentet dyker upp i listan med namn, tid och uppladdare."*

---

### Steg 7 — Ladda upp till modul och aktivitet

**Klicka:** Expandera en modul → "Ladda upp" (vid Moduldokument) → Välj fil → Ladda upp

> *"Samma flöde — men nu kopplas dokumentet till modulen."*

**Klicka:** Upload-ikonen vid en aktivitet → Välj fil → Ladda upp

> *"Och här laddar jag upp direkt till en aktivitet. Varje dokument kopplas till exakt rätt plats — kurs, modul, eller aktivitet."*

---

### Steg 8 — Ta bort ett dokument

**Klicka:** Soptunna-ikonen vid ett dokument.

> *"Läraren kan ta bort dokument. Filen raderas från filsystemet och posten från databasen."*

---

## 📝 Kodgenomgång — Dokumentuppladdning (~3 min)

> *"Nu gör jag en kort kodgenomgång av dokumentuppladdningen — hela flödet från frontend till backend."*

---

### Steg 1: Blazor-komponenten (Frontend)

> *"Vi börjar i frontend. Den här komponenten heter `DocumentUploadModal.razor`. Den använder Blazors inbyggda `InputFile`-komponent:"*

**Visa detta kodblock:**

```razor
<!-- DocumentUploadModal.razor -->

<InputFile OnChange="OnFileSelected" class="form-control" />
```

> *"När användaren väljer en fil anropas `OnFileSelected`. Filen sparas som ett `IBrowserFile`-objekt och valideras direkt i frontend:"*

```csharp
private void OnFileSelected(InputFileChangeEventArgs e)
{
    selectedFile = e.File;
    var (isValid, error) = ValidateFile(selectedFile);
    validationError = isValid ? null : error;
}
```

> *"Valideringen kollar filtillägg mot en vitlista — PDF, DOCX, XLSX och så vidare — och maxstorlek 50 MB. Användaren får direkt feedback."*

---

### Steg 2: Skicka filen (Frontend → Backend)

> *"I `Upload()`-metoden bygger vi en `MultipartFormDataContent` — det är så vi skickar binärdata via HTTP:"*

```csharp
private async Task Upload()
{
    using var content = new MultipartFormDataContent();

    var stream = selectedFile.OpenReadStream(MaxFileSize);
    var streamContent = new StreamContent(stream);
    content.Add(streamContent, "file", selectedFile.Name);

    content.Add(new StringContent(documentName), "name");
    if (CourseId.HasValue)
        content.Add(new StringContent(CourseId.Value.ToString()), "courseId");

    var result = await ApiService.PostMultipartAsync<DocumentDto>(
        "api/upload/document", content);
}
```

> *"Vi öppnar filstreamen, wrappar den i `StreamContent`, och bifogar metadata som namn och kurs-ID. Sedan skickas allt via `PostMultipartAsync` — en metod jag byggt specifikt för filuppladdning. Den skickar requesten till en dedikerad upload-proxy, inte den generella proxyn, på grund av ett tekniskt problem med Blazors Antiforgery-middleware."*

---

### Steg 3: API Controller (Backend)

> *"Requesten landar i `DocumentsController`. Metoden är skyddad med `Authorize(Roles = \"Teacher\")` — bara lärare kan ladda upp:"*

```csharp
[HttpPost("upload")]
[Authorize(Roles = "Teacher")]
[RequestSizeLimit(52_428_800)]   // 50 MB
public async Task<ActionResult<DocumentDto>> Upload(
    [FromForm] DocumentUploadForm form)
{
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

    using var stream = form.File.OpenReadStream();
    var result = await _serviceManager.DocumentService.UploadAsync(
        dto, stream, form.File.FileName, form.File.ContentType,
        form.File.Length, userId);

    return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
}
```

> *"Controllern tar emot filen via `IFormFile`, extraherar användarens ID från JWT-token, och delegerar till service-lagret."*

---

### Steg 4: Service-lagret (Affärslogik)

> *"I `DocumentService.UploadAsync()` sker den viktigaste logiken:"*

```csharp
public async Task<DocumentDto> UploadAsync(...)
{
    // 1. Validera: exakt EN parent-FK (kurs ELLER modul ELLER aktivitet)
    var parentCount = new[] {
        dto.CourseId.HasValue, dto.ModuleId.HasValue, dto.ActivityId.HasValue
    }.Count(x => x);
    if (parentCount != 1)
        throw new ArgumentException("Exactly one parent must be provided.");

    // 2. Validera filtyp + storlek
    if (!AllowedExtensions.Contains(extension))
        throw new ArgumentException("File type not allowed.");

    // 3. Spara filen med unikt GUID-namn (undviker kollisioner)
    var relativePath = await _fileStorage.SaveFileAsync(fileStream, fileName);

    // 4. Skapa Document-entity och spara i databasen
    var document = new Document {
        Name = dto.Name,
        FilePath = relativePath,
        UploadedByUserId = uploadedByUserId,
        CourseId = dto.CourseId,
        ...
    };
    _unitOfWork.DocumentRepository.Create(document);
    await _unitOfWork.CompleteAsync();
}
```

> *"Tre viktiga saker händer här:*  
> *Ett — vi validerar att dokumentet kopplas till exakt en förälder: kurs, modul, eller aktivitet.*  
> *Två — vi validerar filtyp och storlek igen som en andra skyddslinje.*  
> *Tre — filen sparas med ett GUID-baserat filnamn. Det löser problemet med namnkollisioner — om tio användare laddar upp 'schema.pdf' får varje fil ett unikt namn.*  
> *Till sist sparas metadata i databasen via Unit of Work-mönstret."*

---

### Sammanfattning av flödet

> *"Så hela flödet är:"*

```
Blazor InputFile  →  MultipartFormDataContent  →  Upload Proxy
     →  REST API Controller  →  DocumentService  →  Filsystem + Databas
```

> *"Validering sker på tre ställen: frontend, controller, och service. Filen lagras i filsystemet, metadata i SQL Server."*

---

## 🎬 Avslutning (~30 sek)

> *"Det var mina journeys — användarhantering och dokumentpublicering — plus en kodgenomgång av uppladdningen. Allt är full-stack med Blazor WASM och .NET REST API. Nu lämnar jag över till [NÄSTA PERSONS NAMN]."*

---

## 🧠 Reservsvar — vanliga frågor

| Fråga | Kort svar |
|-------|-----------|
| "Var sparas filerna?" | *"I filsystemet i `/uploads/`. Databasen lagrar bara metadata."* |
| "Hur skyddas uppladdningen?" | *"`[Authorize(Roles = \"Teacher\")]` + JWT-token."* |
| "Varför GUID-filnamn?" | *"Förhindrar namnkollisioner vid duplicerade filnamn."* |
| "Varför inte spara i DB?" | *"Branschstandard — DB växer för snabbt med binärdata."* |
| "Kan elever ladda upp?" | *"Ja, men bara inlämningar, inte kursdokument."* |
| "Kan elev skapas utan kurs?" | *"Nej, kurs är obligatorisk. Validering i frontend + backend."* |

---

## ✅ Checklista inför redovisningen

- [ ] Testdata: kurs med moduler och aktiviteter finns
- [ ] Testfil: ha en PDF redo att ladda upp
- [ ] Lärarkonto: kontrollera att inloggning fungerar
- [ ] API + Blazor: starta båda innan demo
- [ ] Kör `taskkill /IM "LMS.API.exe" /F` före start
- [ ] Övning: kör igenom hela demon minst 1 gång
