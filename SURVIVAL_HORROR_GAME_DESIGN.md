# 🎮 Survival Horror Multiplayer/Solo - Game Design Document

## 🎯 Visão Geral do Jogo

**Nome do Projeto:** Dark Survival (nome temporário)
**Gênero:** Survival Horror FPS Multiplayer/Solo
**Jogadores:** 1-5 jogadores
**Engine:** Unity 2022.3 LTS / Unity 6
**Stack:** Netcode for GameObjects + Unity Gaming Services

---

## 📋 Conceito Central

Um jogo de terror em primeira pessoa focado em **sobrevivência e construção de base**, onde jogadores (solo ou grupo de 3-5) precisam sobreviver em um ambiente hostil por **7 dias/noites**, enfrentando monstros que atacam principalmente à noite.

### Loop de Gameplay Principal

```
🌅 DIA 1-7
┌────────────────────────────────────────┐
│ MANHÃ/TARDE (Exploração)               │
│  ├── Explorar o mapa                   │
│  ├── Coletar recursos                  │
│  ├── Caçar/pescar para comida          │
│  └── Investigar locais misteriosos     │
│                                        │
│ TARDE/NOITE (Preparação)               │
│  ├── Voltar para base                  │
│  ├── Construir/reforçar defesas        │
│  ├── Craftar items/armas                │
│  └── Preparar para a noite             │
│                                        │
│ NOITE (Sobrevivência)                  │
│  ├── Defender a base de ondas          │
│  ├── Gerenciar recursos (luz, fogo)    │
│  ├── Cuidar de aliados feridos         │
│  └── Sobreviver até o amanhecer        │
└────────────────────────────────────────┘
```

---

## 🗺️ Cenário Recomendado: **FLORESTA AMALDIÇOADA**

### Por que Floresta? ✅

| Aspecto | Floresta | Manicômio |
|---------|----------|-----------|
| **Espaço de gameplay** | ✅ Grande, exploração livre | ❌ Limitado, claustrofóbico |
| **Construção de base** | ✅ Natural (acampamento) | ❌ Forçado, menos sentido |
| **Coleta de recursos** | ✅ Madeira, pedra, comida | ❌ Limitado a items |
| **Variedade de áreas** | ✅ Lagos, cavernas, ruínas | ⚠️ Corredores similares |
| **Multiplayer** | ✅ Melhor para grupo se dividir | ❌ Pode ficar apertado |
| **Dia/Noite** | ✅ Faz MUITO sentido | ⚠️ Sempre escuro |
| **Atmosfera** | ✅ Isolamento, natureza hostil | ✅ Claustrofobia |
| **Referências** | The Forest, Sons of the Forest | Outlast, Phasmophobia |

### 🏆 Decisão: **FLORESTA + ELEMENTOS DE MANICÔMIO**

**Best of both worlds:**
- Mapa principal: Floresta densa e assustadora
- Locais especiais: Manicômio abandonado, hospital antigo, bunker
- Exploração: Floresta de dia, locais fechados para loot especial

---

## 🎮 Sistemas de Gameplay

### 1. **Sistema de Sobrevivência**

#### Stats Principais
```csharp
// Stats do Player
- ❤️ Vida (Health)
- 🍖 Fome (Hunger) - drena com o tempo, cai mais rápido correndo
- 💧 Sede (Thirst) - drena mais rápido que fome
- 🥶 Temperatura (Temperature) - afetada pelo clima e noite
- 😰 Sanidade (Sanity) - diminui no escuro, perto de monstros
- ⚡ Stamina - regenera quando parado, usada para correr/atacar
```

#### Efeitos de Stats Baixos
```
Fome < 30%:
  - Stamina regenera mais devagar
  - Visão levemente escurecida

Fome < 10%:
  - Perde vida gradualmente
  - Não pode correr

Sede < 30%:
  - Visão embaçada
  - Sons abafados

Sede < 10%:
  - Perde vida rapidamente
  - Movimentação lenta

Temperatura < 30%:
  - Tremores (shake na câmera)
  - Stamina drena mais rápido

Sanidade < 50%:
  - Alucinações visuais leves
  - Sons distorcidos

Sanidade < 20%:
  - Alucinações de monstros (falsos)
  - Tela com efeitos de distorção
  - Pode atrair monstros reais
```

### 2. **Sistema de Construção de Base**

#### Estruturas Construtíveis

**Defesas:**
- 🪵 Parede de Madeira (10 madeira) - HP: 100
- 🪨 Parede de Pedra (15 pedra) - HP: 250, resistente a fogo
- 🚪 Porta Simples (5 madeira) - Pode ser trancada
- 🗼 Torre de Vigia (20 madeira + 10 pedra) - Permite atacar de cima
- ⚡ Armadilha de Espinhos (8 madeira) - Causa dano a monstros
- 🔥 Armadilha de Fogo (5 madeira + óleo) - Queima monstros

**Utilidades:**
- 🔥 Fogueira (5 madeira) - Luz, calor, cozinhar
- 💡 Tocha/Lanterna Estática (3 madeira + óleo) - Iluminação
- 📦 Baú de Armazenamento (10 madeira) - Guarda 20 items
- 🛏️ Cama/Sleeping Bag (15 tecido) - Ponto de respawn
- 💧 Coletor de Água (8 madeira) - Coleta água da chuva
- 🔧 Bancada de Craft (15 madeira + 5 pedra) - Crafts avançados

**Agricultura:**
- 🌱 Plantação (5 madeira + sementes) - Cultiva comida
- 🐟 Armadilha de Pesca (5 madeira) - Pesca automática

#### Sistema de Crafting

**Materiais Base:**
```
Comum:
- 🪵 Madeira (árvores)
- 🪨 Pedra (rochas)
- 🌿 Fibra Vegetal (arbustos)
- 🧵 Tecido (animais, loot)

Avançado:
- ⚙️ Sucata de Metal (manicômio, bunkers)
- 🛢️ Óleo/Combustível (barris, veículos)
- 💊 Medicamentos (farmácias, hospitais)
- 🔋 Baterias (eletrônicos)
```

**Receitas de Craft:**
```
Ferramentas:
- Machado: 5 madeira + 3 pedra
- Picareta: 5 madeira + 5 pedra
- Faca: 2 madeira + 3 pedra
- Lanterna: 5 sucata + 1 bateria

Armas:
- Bastão: 4 madeira
- Lança: 6 madeira + 2 pedra
- Arco: 10 madeira + 5 fibra
- Flechas (5x): 3 madeira + 2 pedra
- Molotov: 1 garrafa + óleo + tecido

Consumíveis:
- Bandagem: 3 tecido
- Kit Médico: 5 tecido + 2 medicamento
- Comida Cozida: carne crua + fogueira
- Água Purificada: água suja + fogueira

Defesas:
- (Ver estruturas acima)
```

### 3. **Sistema de Ciclo Dia/Noite**

```
⏰ Ciclo de 24 minutos (1 dia in-game = 24 min real)
├── 🌅 Amanhecer (05:00-07:00) - 2 min
│   └── Monstros fogem, safe para sair
├── ☀️ Manhã (07:00-12:00) - 5 min
│   └── Exploração principal, luz total
├── 🌤️ Tarde (12:00-17:00) - 5 min
│   └── Continuar exploração
├── 🌆 Entardecer (17:00-19:00) - 2 min
│   └── ⚠️ AVISO: Voltar para base!
├── 🌙 Noite (19:00-23:00) - 6 min
│   └── 🔴 PERIGO MÁXIMO: Ondas de monstros
└── 🌃 Madrugada (23:00-05:00) - 4 min
    └── Última onda, mais intensa
```

**Mecânicas da Noite:**
```
Noite 1: 10-15 monstros, fracos
Noite 2: 15-20 monstros, alguns médios
Noite 3: 20-25 monstros, médios
Noite 4: 25-30 monstros, alguns fortes
Noite 5: 30-40 monstros, vários fortes
Noite 6: 40-50 monstros, muito fortes
Noite 7: 🔴 BOSS FINAL + horda massiva
```

### 4. **Sistema de Inimigos**

#### Tipos de Monstros

**1. Stalker (Perseguidor)**
```
HP: 50
Dano: 10
Velocidade: Média
Comportamento:
  - Observa de longe
  - Ataca quando player está vulnerável
  - Foge se player olha diretamente
  - Som: Respiração pesada
```

**2. Hunter (Caçador)**
```
HP: 75
Dano: 15
Velocidade: Rápida
Comportamento:
  - Aggressivo, persegue ativamente
  - Corre a 4 patas
  - Pula obstáculos
  - Som: Rosnados e uivos
```

**3. Brute (Brutamontes)**
```
HP: 200
Dano: 30
Velocidade: Lenta
Comportamento:
  - Tanque, quebra estruturas
  - Ataque em área
  - Vulnerável nas costas
  - Som: Passos pesados
```

**4. Screamer (Gritador)**
```
HP: 30
Dano: 5 (direto)
Velocidade: Média
Comportamento:
  - Grita para chamar outros monstros
  - Causa dano de sanidade
  - Frágil, mas perigoso em grupo
  - Som: Gritos ensurdecedores
```

**5. Night Terror (BOSS)**
```
HP: 1000
Dano: 50
Velocidade: Variável
Comportamento:
  - Só aparece na Noite 7
  - 3 fases de ataque
  - Invoca outras criaturas
  - Pode destruir estruturas avançadas
```

### 5. **Balanceamento Solo vs Multiplayer**

#### Ajustes Automáticos

**Solo (1 jogador):**
```
✅ Vantagens:
- Menos inimigos nas ondas (-50%)
- Inimigos têm -25% HP
- Mais drops de recursos (+30%)
- Fome/sede drenam -20% mais devagar
- Receitas de craft requerem -20% materiais

❌ Desvantagens:
- Sozinho (risco maior)
- Não pode reviver se morrer
- Precisa gerenciar tudo sozinho
```

**Multiplayer (2-5 jogadores):**
```
Escalamento por jogador:
┌─────────────┬──────────┬─────────┬──────────┐
│ Jogadores   │ Inimigos │ HP      │ Recursos │
├─────────────┼──────────┼─────────┼──────────┤
│ 2 players   │ +30%     │ +15%    │ +20%     │
│ 3 players   │ +60%     │ +30%    │ +40%     │
│ 4 players   │ +90%     │ +45%    │ +60%     │
│ 5 players   │ +120%    │ +60%    │ +80%     │
└─────────────┴──────────┴─────────┴──────────┘

✅ Vantagens:
- Pode dividir tarefas
- Sistema de revive (60s para reviver)
- Mais firepower
- Compartilha recursos

❌ Desvantagens:
- Mais inimigos
- Inimigos mais fortes
- Recursos por pessoa são iguais
- Precisa coordenação
```

#### Sistema de Revive (Multiplayer)
```
Quando player morre:
1. Entra em estado "Down" (caído)
2. Tem 60 segundos antes de morte definitiva
3. Aliado pode reviver (5s channeling)
4. Se revivido: 30% HP, 50% stamina
5. Se morrer de vez: respawn na próxima manhã
```

### 6. **Progressão e Objetivos**

#### Objetivos Principais

**História Principal:**
```
Dia 1-2: Estabelecer base, sobreviver primeiras noites
Dia 3-4: Explorar manicômio/bunker, descobrir a origem dos monstros
Dia 5-6: Encontrar maneira de escapar (rádio, veículo, etc)
Dia 7: Enfrentar boss final E escapar ou realizar ritual
```

**Objetivos Opcionais:**
```
- 🏆 Encontrar todos os 10 diários espalhados
- 🏆 Construir base tier 3 (paredes de pedra)
- 🏆 Derrotar 100 monstros
- 🏆 Sobreviver sem morrer
- 🏆 Completar todos os 7 dias
```

#### Sistema de Unlock

**Receitas de Craft Avançadas:**
```
Desbloqueadas encontrando blueprints no mapa:
- 📘 Blueprint: Armadilha Elétrica (bunker)
- 📘 Blueprint: Escopeta Modificada (delegacia)
- 📘 Blueprint: Bomba Caseira (laboratório)
- 📘 Blueprint: Armadura Reforçada (oficina)
```

---

## 🎨 Direção de Arte e Atmosfera

### Ambientação - Floresta Amaldiçoada

**Biomas:**
```
1. 🌲 Floresta Densa (Centro)
   - Árvores altas bloqueiam luz
   - Névoa densa
   - Sons de animais distorcidos

2. 🏚️ Área Urbana Abandonada
   - Manicômio principal (3 andares)
   - Hospital destruído
   - Delegacia saqueada
   - Casas em ruínas

3. 🏞️ Lago Sombrio
   - Água escura, reflexos estranhos
   - Píer destruído
   - Pesca disponível

4. ⛰️ Cavernas
   - Mineração de recursos
   - Criaturas especiais
   - Totalmente escuro

5. 🏭 Bunker Militar
   - Loot avançado
   - Desafio extra
   - Energia elétrica funcional (limitada)
```

### Estilo Visual

**Iluminação:**
- Muito escura à noite (realista)
- Fog volumétrico
- Sombras dinâmicas
- Luz de fogo oscilante
- Efeitos de raios de luz (god rays) na floresta

**Pós-Processamento:**
- Vignette sutil
- Chromatic aberration leve
- Film grain
- Color grading dessaturado (tons de verde e cinza)

**Áudio:**
- Som ambiente 3D (vento, folhas, água)
- Sistema de música adaptativa (tensão)
- Sons de monstros direcionais
- Reverb em áreas fechadas

---

## 🛠️ Stack Técnico Recomendado

### Unity Setup

```
Unity Version: 2022.3 LTS
Render Pipeline: URP (melhor performance)

Packages Essenciais:
✅ Netcode for GameObjects 1.8.0+
✅ Unity Transport 2.0+
✅ Unity Gaming Services (Lobby + Relay)
✅ Input System 1.7.0+
✅ Cinemachine 2.9+
✅ Post Processing 3.2+
✅ ProBuilder (para prototipar geometria)
✅ TextMeshPro
```

### Arquitetura de Código

```
Patterns:
- Event Bus (comunicação desacoplada)
- GOAP AI (IA dos monstros)
- Object Pooling (inimigos, projéteis, efeitos)
- State Machine (estados do jogo)
- Observer Pattern (sistema de eventos)
- ScriptableObjects (dados configuráveis)

Namespace:
SurvivalHorror.Core
SurvivalHorror.Player
SurvivalHorror.Enemy
SurvivalHorror.Building
SurvivalHorror.Survival
SurvivalHorror.Networking
SurvivalHorror.Environment
SurvivalHorror.UI
```

---

## 📊 Performance Targets

```
Target Platform: PC (pode expandir para console)
Target FPS: 60 FPS (mínimo 30 FPS)
Max Players: 5
Max Enemies Simultâneos: 50
Max Structures: 200
Viewdistance: 150m (floresta), 50m (indoor)

Otimizações:
- LOD em todos modelos
- Occlusion Culling
- Object Pooling
- Batching de meshes estáticos
- Async loading de áreas
- Compressão de texturas
```

---

## 🗓️ Roadmap de Desenvolvimento (16 Semanas)

### Semana 1-2: Foundation
- ✅ Setup projeto Unity
- ✅ Integrar Netcode + UGS
- ✅ Input System configurado
- ✅ Player controller básico

### Semana 3-4: Core Gameplay
- Sistema de sobrevivência (stats)
- Ciclo dia/noite funcional
- Coleta de recursos básica
- Sistema de crafting base

### Semana 5-6: Combat & Enemies
- Sistema de combate
- IA básica de inimigos (3 tipos)
- Sistema de dano/morte
- Wave system para noite

### Semana 7-8: Building System
- Sistema de construção
- 10 estruturas funcionais
- Grid de placement
- Durabilidade de estruturas

### Semana 9-10: Multiplayer
- Sincronização de players
- Sincronização de inimigos
- Sistema de lobby
- Balanceamento MP vs Solo

### Semana 11-12: World & Content
- Mapa da floresta (500x500m)
- 5 biomas
- Locais de interesse (5+)
- Sistema de loot

### Semana 13-14: Polish & Features
- UI/UX completo
- Áudio completo
- Atmosfera e iluminação
- Efeitos visuais

### Semana 15-16: Testing & Balance
- Playtesting
- Balanceamento
- Bug fixes
- Otimização
- Build final

---

## 🎯 MVP (Minimum Viable Product)

Para lançar uma primeira versão jogável:

**Essencial:**
- [x] Player FPS funcional (já tem)
- [ ] Sistema de sobrevivência (3 stats: vida, fome, sede)
- [ ] Coleta de 3 recursos (madeira, pedra, comida)
- [ ] Crafting de 5 items básicos
- [ ] Construção de 5 estruturas
- [ ] Ciclo dia/noite funcional
- [ ] 2 tipos de inimigos
- [ ] Sistema de ondas noturnas
- [ ] Multiplayer 2-4 players
- [ ] Mapa 250x250m com floresta

**Pode adiar:**
- Sanidade
- Temperatura
- Boss final
- Biomas extras
- Locais especiais
- Storyline completa

---

## 🚀 Próximos Passos Imediatos

1. **Criar documento de arquitetura técnica detalhada**
2. **Implementar sistema de sobrevivência**
3. **Implementar sistema de construção**
4. **Criar protótipo do ciclo dia/noite**
5. **Integrar sistema de balanceamento dinâmico**

---

**Este documento serve como guia completo para o desenvolvimento. Todos os sistemas serão implementados usando as melhores práticas modernas de Unity e arquitetura escalável.**
