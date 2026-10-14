# ContainerFlowAPI

Este repositório serve como um projeto de estudo e aplicação prática de conceitos de Domain-Driven Design (DDD) e arquitetura de software moderna.
O objetivo foi construir uma base sólida para qualquer backend, utilizando as melhores práticas do ecossistema .NET, com ênfase na limpeza e separação de responsabilidades.

## Princípios de Arquitetura e DDD

O código é organizado em Bounded Contexts, que agem como módulos independentes, cada um resolvendo uma parte específica do negócio. Dentro de cada contexto, usamos Agregados para garantir a consistência das regras de negócio. 
A comunicação entre os componentes é feita de forma desacoplada através de Domain Events, que sinalizam eventos importantes que já aconteceram. 
Por fim, uma Camada Anti-Corrupção (ACL) protege o nosso domínio de modelos externos, traduzindo a comunicação com sistemas de terceiros e legados.

## Documentação de Arquitetura

Para uma análise detalhada dos Bounded Contexts, fluxo de **Transactional Outbox/Inbox** e diagramas Mermaid renderizáveis, consulte o arquivo [ARCHITECTURE.md](ARCHITECTURE.md).

## Projeto em Construção

Este projeto é um trabalho em andamento e está em sua fase de desenvolvimento. Se você está interessado em contribuir para um projeto real, aprender e colocar em prática conceitos de Domain-Driven Design (DDD) e Arquitetura Limpa, sinta-se à vontade para contribuir com melhorias!
