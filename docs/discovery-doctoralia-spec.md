# Discovery publico inspirado na Doctoralia

## Referencia analisada

Fonte principal: https://www.doctoralia.com.br/, consultada em 2026-06-04.

A pagina publica da Doctoralia organiza a experiencia do paciente em torno de:

- busca inicial por especialidade, doenca ou nome;
- busca por cidade ou regiao;
- alternancia entre atendimento presencial e teleconsulta;
- atalhos para especialidades populares;
- separacao clara entre area do paciente e area de especialistas/clinicas;
- listas de especialistas e clinicas com nome, especialidades, endereco, mapa, opinioes, servicos, precos, disponibilidade e link de perfil;
- fluxo simples: encontrar especialista, escolher profissional/dia/horario, receber lembretes e avaliar depois.

## Principios para o HealthTech

O HealthTech deve usar a mesma clareza de descoberta, mas com controles de seguranca e operacao melhores:

- IDs sao contrato tecnico; a UI publica exibe nomes, regioes, especialidades, profissionais e horarios.
- A busca publica nunca acessa prontuario, pacientes internos, faturamento ou dados de tenant operacional.
- A Area da Clinica fica separada do discovery publico e exige sessao BFF com membership.
- A localizacao do navegador so e solicitada por acao explicita do paciente.
- A busca por proximidade usa latitude/longitude das unidades publicas cadastradas, sem servico externo de mapas na V1.
- O agendamento publico grava apenas dados minimos do paciente e valida clinica, profissional, unidade e conflito no banco.

## Experiencia alvo

### Home publica

A primeira tela deve oferecer quatro caminhos equivalentes:

- **Especialidade/medico**: o paciente digita "Cardiologia", "Dra. Laura" ou termos parecidos e ve profissionais.
- **Clinica**: o paciente digita o nome da clinica e ve unidades/profissionais daquela clinica.
- **Regiao por texto**: o paciente informa cidade, bairro, estado, CEP parcial ou regiao publica.
- **Perto de mim**: o paciente clica em "Usar minha localizacao"; o navegador pede permissao e o BFF ordena unidades proximas.

### Resultados

Cada card publico deve mostrar:

- nome do profissional;
- especialidade;
- clinica;
- unidade;
- cidade/estado/regiao;
- proximos horarios;
- estado sem horario quando nao houver slot no periodo calculado.

Proximos incrementos planejados para superar a referencia:

- filtros por convenio e tipo de pagamento;
- preco por servico;
- foto/verificacao do profissional;
- perfil publico da clinica/profissional;
- reputacao e avaliacoes verificadas;
- lembretes por WhatsApp/e-mail;
- teleconsulta real quando existir modelo de atendimento remoto.

## Contratos publicos

### GET /api/discovery/search

Parametros:

- `mode=professional|clinic`
- `query`
- `clinic`
- `specialty`
- `region`
- `latitude`
- `longitude`
- `take`

Regras:

- `take` fica limitado entre 1 e 50.
- `query`, `clinic`, `specialty` e `region` aceitam ate 120 caracteres.
- latitude exige longitude e vice-versa.
- latitude aceita -90 a 90; longitude aceita -180 a 180.
- quando latitude/longitude existem, resultados com unidade georreferenciada aparecem primeiro por proximidade aproximada.
- quando `region` existe, o resultado tambem precisa casar cidade, bairro, estado, CEP ou regiao publica.

### POST /api/discovery/appointments

O browser envia IDs tecnicos apenas para executar a selecao feita na UI:

- `tenantId`
- `professionalId`
- `locationId` opcional
- horario escolhido
- dados minimos de contato do paciente

O banco valida se clinica, profissional e unidade estao ativos e se a unidade pertence a clinica informada.

## Separacao Area da Clinica

O acesso operacional fica fora do discovery publico:

- visitante usa **Area da clinica** em `/login?accessArea=clinic`;
- apos login, usuarios com memberships acessam **Minhas clinicas** em `/minhas-clinicas`;
- a troca de clinica ativa chama o BFF e atualiza a sessao HttpOnly;
- menus internos sao derivados das permissoes retornadas pelo BFF.

## Diferenciais planejados

Para ser melhor que a referencia, o HealthTech deve priorizar:

- busca com seguranca multi-tenant desde o banco;
- agenda real da clinica integrada ao BFF;
- acesso operacional separado e auditavel;
- onboarding de clinicas controlado pelo SystemAdmin;
- compatibilidade futura com IA de triagem, sem permitir que IA autorize dados sensiveis;
- teleconsulta, convenio, preco e avaliacao como modulos incrementais, nao como campos soltos no frontend.
