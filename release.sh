#!/usr/bin/env bash
# Intentum release: son tag + conventional commit'lere göre sürümü otomatik artırır, tag push eder.
# Kullanım: ./release.sh           → otomatik bump (BREAKING→major, feat→minor, diğer→patch)
#          ./release.sh 1.2.0      → elle sürüm (otomatik hesaplamayı atla)
#
# Conventional commit: feat: = minor, fix:/docs/chore = patch, BREAKING veya feat!: = major

set -e

# Son v* tag'ini bul (yoksa 0.0.0)
LATEST_TAG=$(git describe --tags --abbrev=0 --match 'v*' 2>/dev/null || true)
if [[ -z "$LATEST_TAG" ]]; then
  LATEST_TAG="v0.0.0"
fi

# major.minor.patch çıkar
VERSION_PART="${LATEST_TAG#v}"
MAJOR=$(echo "$VERSION_PART" | sed -n 's/^\([0-9]*\).*/\1/p')
MINOR=$(echo "$VERSION_PART" | sed -n 's/^[0-9]*\.\([0-9]*\).*/\1/p')
PATCH=$(echo "$VERSION_PART" | sed -n 's/^[0-9]*\.[0-9]*\.\([0-9]*\).*/\1/p')
MAJOR=${MAJOR:-0}
MINOR=${MINOR:-0}
PATCH=${PATCH:-0}

# Elle sürüm verilmişse onu kullan
if [[ -n "$1" ]]; then
  NEXT_TAG="$1"
  case "$NEXT_TAG" in
    v*) ;;
    *) NEXT_TAG="v$NEXT_TAG" ;;
  esac
  echo "Elle sürüm: $NEXT_TAG (son tag: $LATEST_TAG)"
else
  # Son tag'den bu yana commit mesajlarına bak (subject + body)
  COMMITS=$(git log "$LATEST_TAG..HEAD" --pretty=format:"%s%n%b" 2>/dev/null || true)
  BUMP="patch"

  if [[ -n "$COMMITS" ]]; then
    # BREAKING CHANGE (body) veya feat!: fix!: (subject) → major
    if echo "$COMMITS" | grep -qEi 'BREAKING CHANGE|^[a-zA-Z]*!:'; then
      BUMP="major"
    # feat: (breaking değil) → minor
    elif echo "$COMMITS" | grep -qE '^feat(\([^)]*\))?:'; then
      BUMP="minor"
    fi
  fi

  case "$BUMP" in
    major)
      MAJOR=$((MAJOR + 1))
      MINOR=0
      PATCH=0
      ;;
    minor)
      MINOR=$((MINOR + 1))
      PATCH=0
      ;;
    patch)
      PATCH=$((PATCH + 1))
      ;;
    *)
      echo "Unexpected bump value: $BUMP"; exit 1
      ;;
  esac

  NEXT_TAG="v${MAJOR}.${MINOR}.${PATCH}"
  echo "Son tag: $LATEST_TAG → Yeni sürüm: $NEXT_TAG (bump: $BUMP)"
fi

printf "Bu tag'i oluşturup origin'e push edeceğim. Devam? [y/N] "
read -r reply
case "$reply" in
  [yY][eE][sS]|[yY]) ;;
  *) echo "İptal."; exit 0 ;;
esac

git tag "$NEXT_TAG"
git push origin "$NEXT_TAG"

echo "Tamam. GitHub Actions'ta github-release ve nuget-release çalışacak."
