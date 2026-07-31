# Analyse préalable de MonTresor

Le dépôt MonTresor n'était pas présent dans `/workspace` et la tentative d'accès réseau à `https://github.com/ArnaudCaroulle/MonTresor.git` a échoué avec une réponse `CONNECT tunnel failed, response 403`.

Conséquence : cette première version de MonPlan est initialisée avec une architecture ASP.NET Core MVC standard, compatible IONOS/MySQL, sans modification de MonTresor.
