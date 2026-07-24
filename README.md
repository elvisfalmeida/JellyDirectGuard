<p align="center">
  <img src="jellyguard_logo.png" alt="JellyDirectGuard" width="220"/>
</p>

<h1 align="center">JellyDirectGuard</h1>

<p align="center">
  <a href="https://github.com/elvisfalmeida/JellyDirectGuard/releases/latest"><img src="https://img.shields.io/github/v/release/elvisfalmeida/JellyDirectGuard?label=release" alt="Release"/></a>
  <img src="https://img.shields.io/badge/Jellyfin-10.11%2B-8b5cf6" alt="Jellyfin 10.11+"/>
  <img src="https://img.shields.io/badge/license-GPL--3.0-blue" alt="GPL-3.0"/>
</p>

Plugin para **Jellyfin 10.11+** que força **direct play**: desativa a
transcodificação de vídeo na política dos usuários, automaticamente — inclusive
para contas novas criadas por ferramentas como o
[Wizarr](https://github.com/wizarrrr/wizarr). Ideal para servidores **sem GPU**,
onde um único transcode de vídeo satura a CPU, enquanto direct play é só I/O de
rede.

> Plugin for Jellyfin 10.11+ that enforces direct play by disabling video
> transcoding in user policies — applied in real time to newly created users
> (e.g. by Wizarr). Made for GPU-less servers where a single video transcode
> can saturate the CPU.

## Por que

Sem aceleração de hardware, 2–3 transcodes simultâneos derrubam um servidor
pequeno. Se a biblioteca já é H.264/AAC/MP4, direct play atende dezenas de
espectadores na mesma máquina. O Jellyfin, porém, **não tem política padrão
para usuários novos** — cada conta nasce com transcode liberado. Este plugin
fecha essa brecha.

## Recursos

- 🚫 Bloqueia transcodificação de vídeo por política de usuário (sem tocar no ffmpeg);
- ⚡ Aplica **em tempo real** ao evento de criação de usuário;
- 🔁 Re-verificação configurável após a criação — pega ferramentas (Wizarr etc.)
  que gravam a própria política logo depois de criar a conta;
- 🧹 Varredura periódica interna (padrão 10 min) + tarefa agendada (12 h) +
  varredura ao iniciar o servidor: edições manuais de política não permanecem;
- 🎛️ Remux (troca de container) e transcode de áudio continuam permitidos
  (configurável) — AC3→AAC é barato e às vezes necessário;
- 👑 Administradores intocados por padrão (válvula de escape para diagnóstico);
- 🚷 Lista de usuários ignorados por nome;
- 📊 Tabela de status no Dashboard mostrando a política de cada usuário, com
  botão **Aplicar agora**.

## Instalação

### Pelo catálogo (recomendado)

1. Dashboard → Plugins → Repositórios → **Adicionar**;
2. URL do repositório: `https://elvisfalmeida.github.io/JellyDirectGuard/manifest.json`;
3. Catálogo → **JellyDirectGuard** → Instalar → reiniciar o Jellyfin.

### Manual

1. Baixe o `jellydirectguard_X.Y.Z.zip` da release;
2. Extraia em `config/plugins/JellyDirectGuard_X.Y.Z/`;
3. Reinicie o Jellyfin.

## Configuração

Dashboard → Plugins → JellyDirectGuard:

| Campo | Descrição |
|---|---|
| Ativar aplicação automática | Liga/desliga o plugin sem desinstalar |
| Bloquear transcodificação de vídeo | O coração do plugin |
| Permitir remux / transcode de áudio | Operações baratas, mantidas por padrão |
| Não tocar em administradores | Admins seguem com transcode (padrão) |
| Usuários ignorados | Nomes que o plugin nunca altera |
| Re-verificação após criação | Segundos até a segunda passada (padrão 60) |
| Varredura periódica | Minutos entre varreduras completas (padrão 10) |

## Como funciona

O plugin consome o evento `UserCreated` do Jellyfin e clampa a política da
conta no mesmo segundo. Como ferramentas de convite costumam gravar a própria
política logo após criar o usuário — e edições de política não geram evento
visível a plugins — há três redes de segurança: a re-verificação pós-criação,
a varredura periódica interna e a tarefa agendada. Tudo idempotente: só
escreve quando o usuário está fora da regra.

Um cliente que exija bitrate menor que o do arquivo falha na hora, em vez de
derrubar o servidor para todo mundo. Mantenha a biblioteca em
H.264 + AAC + MP4 e o direct play é universal.

## Compilar do código-fonte

```bash
dotnet build -c Release Jellyfin.Plugin.JellyDirectGuard
# DLL em Jellyfin.Plugin.JellyDirectGuard/bin/Release/net9.0/
```

Para gerar o zip de release e atualizar o manifest:

```bash
./scripts/package.sh 1.0.0.0 "changelog"
```

## Requisitos

- Jellyfin **10.11** ou superior.

## Veja também

- [JellyWahaNotify](https://github.com/elvisfalmeida/JellyWahaNotify) —
  notificações de WhatsApp (conteúdo novo, logins e reproduções) via WAHA.

## Créditos

Criado por [Elvis Almeida](https://github.com/elvisfalmeida) e desenvolvido em
parceria com **Claude** (Anthropic), via [Claude Code](https://claude.com/claude-code) —
da arquitetura ao código, testes em produção e automação de release.

Licença [GPL-3.0](LICENSE): use, estude, modifique e redistribua livremente.
