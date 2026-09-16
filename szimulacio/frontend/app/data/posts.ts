export type Post = {
  postId: number;
  title: string;
  slug: string;
  excerpt: string;
  authorName: string;
  publishedAtUtc: string;
  commentCount: number;
};

export const posts: Post[] = [
  {
    postId: 1,
    title: "A palacsinta, ami végre nem szakadt el",
    slug: "palacsinta-ami-nem-szakad",
    excerpt:
      "Évekig azt hittem, a serpenyővel van baj. Kiderült, hogy a tésztát " +
      "egyszerűen hagyni kell pihenni fél órát, és minden más magától megoldódik.",
    authorName: "Varga Júlia",
    publishedAtUtc: "2026-09-10T08:30:00Z",
    commentCount: 7,
  },
  {
    postId: 2,
    title: "A Csillagos éj nem az égboltról szól",
    slug: "csillagos-ej-van-gogh",
    excerpt:
      "Mindenki a kavargó kékeket nézi. Pedig a kép alján ott alszik egy " +
      "falu, amit Van Gogh odaképzelt — az ablakából nem ez látszott.",
    authorName: "Barna Gergő",
    publishedAtUtc: "2026-09-08T17:45:00Z",
    commentCount: 4,
  },
  {
    postId: 3,
    title: "Öt hobbi, amit idén el akartam kezdeni",
    slug: "ot-hobbi-amit-el-akartam-kezdeni",
    excerpt:
      "Kettőből lett valami, egyet két hét után feladtam, kettőhöz hozzá sem " +
      "kezdtem. Számba vettem, mi döntötte el, melyik melyik lett.",
    authorName: "Varga Júlia",
    publishedAtUtc: "2026-09-03T06:00:00Z",
    commentCount: 2,
  },
  {
    postId: 4,
    title: "Szeptemberi eső, nyitott ablak",
    slug: "szeptemberi-eso-nyitott-ablak",
    excerpt:
      "Az idei nyár úgy ért véget, hogy egyik nap még huszonnyolc fok volt, " +
      "másnap pedig elő kellett keresni a pulóvereket a szekrény tetejéről.",
    authorName: "Barna Gergő",
    publishedAtUtc: "2026-08-28T19:05:00Z",
    commentCount: 0,
  },
  {
    postId: 5,
    title: "A lefelé a nehezebb, nem a felfelé",
    slug: "lefele-a-nehezebb",
    excerpt:
      "Hét kilométer ereszkedés után jöttem rá, hogy rossz bakancsot hoztam. " +
      "Azóta túra előtt nem a szintemelkedést nézem meg először.",
    authorName: "Varga Júlia",
    publishedAtUtc: "2026-08-21T11:20:00Z",
    commentCount: 3,
  },
];
