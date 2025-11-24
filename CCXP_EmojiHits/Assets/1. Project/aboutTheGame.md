Este é um jogo para PC no qual, teremos duas telas de jogo, sendo uma para um staff, e outra para o jogador.


O jogo consiste em mostrar uma sequencia de emojis (que deve ser um PNG na pasta streamingassets) na tela de jogo, na tela do staff, devemos mostrar o nome da musica/autor, e no momento em que o jogador responder, o staff pode apertar em acertou ou errou. 

ou seja, vamos ter uma estrutura de arquivos para esse jogo que deve ser algo proximo disso:
musica: nome...
autor: nome...
letra: letra...
arquivoImagemEmoji: nomeDoArquivo.png
arquivoMusica: nomeDoArquivo.mp3 ou .wav


apos a resposta, vamos ter uma tela de feedback. mostrando se a pessoa acertou ou errou. em caso de acerto. vamos ter uma tela de vitoria, na qual se toca a musica.

seria interessante, ter uma metrica de qntas vezes x musica foi carregada, x vezes q acertaram e x vezes q erram. quem sabe isso vira um .csv na streamingassets.