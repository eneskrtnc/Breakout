# JSON Veri Şeması (v0)

Kök: `Assets/Resources/data/*.json` • Kök tip: **array**.  
Zorunlu alanlar: `id: string`, `displayName: string`, `desc: string`,  
`tags: string[]`, `numbers: { [key: string]: number }`

## Örnek kayıt
```json
{
  "id": "starter_ship",
  "displayName": "Starter Shuttle",
  "desc": "Yeni oyuncular için giriş seviyesi gemi.",
  "tags": ["starter", "civilian"],
  "numbers": { "hp": 100, "cargo": 8, "speed": 12.5 }
}
